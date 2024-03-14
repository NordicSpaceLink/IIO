// Copyright (C) 2024 - Nordic Space Link
using System;
using System.Collections.Generic;
using System.Linq;

namespace NordicSpaceLink.IIO
{
    public class Context : NativeObject
    {
        private unsafe class ContextAttribute : Attribute
        {
            private byte* valuePtr;

            public ContextAttribute(string name, byte* value) : base(name)
            {
                valuePtr = value;
            }

            protected override string GetValue()
            {
                return Common.ConstCString(valuePtr, 1024);
            }
        }

        private IntPtr handle;

        private unsafe Context(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Failed to create context");

            this.handle = handle;

            // Enumerate devices
            List<(Device, string[])> devs = new();

            var count = NativeMethods.iio_context_get_devices_count(handle);
            for (int i = 0; i < (int)count; i++)
            {
                var dev = new Device(this, NativeMethods.iio_context_get_device(handle, (nuint)i));
                devs.Add((dev, dev.GetLabels()));
            }

            Devices = new Indexer<string, Device>(devs);

            // Add attributes
            List<(Attribute, string[])> attrs = new();

            count = NativeMethods.iio_context_get_attrs_count(handle);
            for (int i = 0; i < (int)count; i++)
            {
                byte* namePtr, valuePtr;
                Common.CheckError(NativeMethods.iio_context_get_attr(handle, (nuint)i, &namePtr, &valuePtr));

                if ((namePtr != null) && (valuePtr != null))
                {
                    var name = Common.ConstCString(namePtr, 1024);
                    attrs.Add((new ContextAttribute(name, valuePtr), new string[] { name }));
                }
            }

            Attributes = new Indexer<string, Attribute>(attrs);
        }

        public Indexer<string, Device> Devices { get; }
        public Indexer<string, Attribute> Attributes { get; }

        /// <summary>
        /// Set a timeout for I/O operations. A timespan of 0 or less than a millisecond will result in no timeout.
        /// </summary>
        public TimeSpan Timeout
        {
            set
            {
                Common.CheckError(NativeMethods.iio_context_set_timeout(handle, (nuint)value.TotalMilliseconds));
            }
        }

        /// <summary>
        /// Create a context from local or remote IIO devices.
        /// 
        ///  This function will create a network context if the IIOD_REMOTE
        ///  environment variable is set to the hostname where the IIOD server runs. If
        ///  set to an empty string, the server will be discovered using ZeroConf.
        ///  If the environment variable is not set, a local context will be created
        ///  instead.
        /// </summary>
        public static Context Default => new(NativeMethods.iio_create_default_context());

        /// <summary>
        /// Create a context from local IIO devices (Linux only).
        /// </summary>
        public static Context Local => new(NativeMethods.iio_create_local_context());

        /// <summary>
        /// Create a context from the network
        /// </summary>
        /// <param name="address">Hostname, IPv4 or IPv6 address where the IIO Daemon is running</param>
        public static Context Network(string address) => new(NativeMethods.iio_create_network_context(address));

        /// <summary>
        /// Create a context from a URI description
        /// </summary>
        /// <param name="uri">A URI describing the context location. Refer to "iio_create_context_from_uri" for documentation for syntax.</param>
        public static Context FromURI(string uri) => new(NativeMethods.iio_create_context_from_uri(uri));

        protected override void DoDispose()
        {
            Devices.ToList().ForEach(dev => dev.DoDispose());
        }

        protected override void Free()
        {
            NativeMethods.iio_context_destroy(handle);
            handle = IntPtr.Zero;
        }
    }
}
