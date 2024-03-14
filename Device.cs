// Copyright (C) 2024 - Nordic Space Link
using System;
using System.Collections.Generic;
using System.Linq;

namespace NordicSpaceLink.IIO
{
    public class Device
    {
        private class DeviceAttribute : Attribute
        {
            private readonly IntPtr dev;

            public DeviceAttribute(IntPtr dev, string name) : base(name)
            {
                this.dev = dev;
            }

            protected unsafe override string GetValue()
            {
                const int buffersize = 4096;
                byte* buffer = stackalloc byte[buffersize];

                var size = Common.CheckError(NativeMethods.iio_device_attr_read(dev, Name, buffer, buffersize));

                return new Span<byte>(buffer, (int)size).CString();
            }

            protected override void SetValue(string value)
            {
                Common.CheckError(NativeMethods.iio_device_attr_write(dev, Name, value));
            }
        }

        private class DeviceBufferAttribute : Attribute
        {
            private readonly IntPtr dev;

            public DeviceBufferAttribute(IntPtr dev, string name) : base(name)
            {
                this.dev = dev;
            }

            protected unsafe override string GetValue()
            {
                const int buffersize = 4096;
                byte* buffer = stackalloc byte[buffersize];

                var size = Common.CheckError(NativeMethods.iio_device_buffer_attr_read(dev, Name, buffer, buffersize));

                return new Span<byte>(buffer, (int)size).CString();
            }

            protected override void SetValue(string value)
            {
                Common.CheckError(NativeMethods.iio_device_buffer_attr_write(dev, Name, value));
            }
        }

        private class DeviceDebugAttribute : Attribute
        {
            private readonly IntPtr dev;

            public DeviceDebugAttribute(IntPtr dev, string name) : base(name)
            {
                this.dev = dev;
            }

            protected unsafe override string GetValue()
            {
                const int buffersize = 4096;
                byte* buffer = stackalloc byte[buffersize];

                var size = Common.CheckError(NativeMethods.iio_device_debug_attr_read(dev, Name, buffer, buffersize));

                return new Span<byte>(buffer, (int)size).CString();
            }

            protected override void SetValue(string value)
            {
                Common.CheckError(NativeMethods.iio_device_debug_attr_write(dev, Name, value));
            }
        }

        private readonly IntPtr device;
        private readonly string[] labels;

        private readonly List<Buffer> buffers = new();

        public Context Context { get; }

        public string Name { get; }
        public string ID { get; }

        /// <summary>
        /// Get the current sample size
        /// </summary>
        /// <remarks>The sample size is not constant and will change when channels get enabled or disabled</remarks>
        public int SampleSize { get => (int)Common.CheckError(NativeMethods.iio_device_get_sample_size(device)); }

        public Indexer<string, Attribute> Attributes { get; }
        public Indexer<string, Attribute> BufferAttributes { get; }
        public Indexer<string, Attribute> DebugAttributes { get; }

        public Indexer<(string label, bool isOutput), Channel> Channels { get; }

        public Channel FindChannel(string label, bool isOutput) => Channels[(label, isOutput)];

        /// <summary>
        /// Create an input or output buffer associated to the given device
        /// </summary>
        /// <param name="sample_count">The number of samples that the buffer should contain</param>
        /// <param name="cyclic">If True, enable cyclic mode</param>
        /// <remarks>Channels that have to be written to / read from must be enabled before creating the buffer</remarks>
        public Buffer CreateBuffer(int sample_count, bool cyclic)
        {
            var bufferPtr = NativeMethods.iio_device_create_buffer(device, (nuint)sample_count, cyclic);

            if (bufferPtr == null)
                throw new Exception("Could not allocate buffer");

            var buffer = new Buffer(this, bufferPtr);
            buffers.Add(buffer);

            return buffer;
        }

        internal string[] GetLabels()
        {
            return labels;
        }

        internal unsafe Device(Context context, IntPtr device)
        {
            this.device = device;
            Context = context;

            Name = Common.ConstCString(NativeMethods.iio_device_get_name(device), 1024);
            ID = Common.ConstCString(NativeMethods.iio_device_get_id(device), 1024);

            var result = new List<string>();

            if (!string.IsNullOrWhiteSpace(Name))
                result.Add(Name);
            if (!string.IsNullOrWhiteSpace(ID))
                result.Add(ID);

            labels = result.ToArray();

            // Device attributes
            var attrs = new List<(Attribute, string[])>();

            var count = NativeMethods.iio_device_get_attrs_count(device);
            for (int i = 0; i < (int)count; i++)
            {
                var namePtr = NativeMethods.iio_device_get_attr(device, (nuint)i);

                if (namePtr != null)
                {
                    var name = Common.ConstCString(namePtr, 1024);
                    attrs.Add((new DeviceAttribute(device, name), new string[] { name }));
                }
            }

            Attributes = new Indexer<string, Attribute>(attrs);

            // Device buffer attributes
            attrs = new List<(Attribute, string[])>();

            count = NativeMethods.iio_device_get_buffer_attrs_count(device);
            for (int i = 0; i < (int)count; i++)
            {
                var namePtr = NativeMethods.iio_device_get_buffer_attr(device, (nuint)i);

                if (namePtr != null)
                {
                    var name = Common.ConstCString(namePtr, 1024);
                    attrs.Add((new DeviceBufferAttribute(device, name), new string[] { name }));
                }
            }

            BufferAttributes = new Indexer<string, Attribute>(attrs);

            // Device debug attributes
            attrs = new List<(Attribute, string[])>();

            count = NativeMethods.iio_device_get_debug_attrs_count(device);
            for (int i = 0; i < (int)count; i++)
            {
                var namePtr = NativeMethods.iio_device_get_debug_attr(device, (nuint)i);

                if (namePtr != null)
                {
                    var name = Common.ConstCString(namePtr, 1024);
                    attrs.Add((new DeviceDebugAttribute(device, name), new string[] { name }));
                }
            }

            DebugAttributes = new Indexer<string, Attribute>(attrs);

            // Channels
            var chans = new List<(Channel, (string, bool)[])>();

            count = NativeMethods.iio_device_get_channels_count(device);
            for (int i = 0; i < (int)count; i++)
            {
                var chn = NativeMethods.iio_device_get_channel(device, (nuint)i);

                var cName = Common.ConstCString(NativeMethods.iio_channel_get_name(chn), 1024);
                var cID = Common.ConstCString(NativeMethods.iio_channel_get_id(chn), 1024);

                var chan = new Channel(this, chn, cName, cID);

                chans.Add((chan, chan.GetLabels()));
            }

            Channels = new Indexer<(string, bool), Channel>(chans);
        }

        public override string ToString()
        {
            return string.Join(",", labels);
        }

        internal void FreeBuffer(Buffer buffer)
        {
            buffers.Remove(buffer);
        }

        internal void DoDispose()
        {
            foreach (var buf in buffers.ToList())
                buf.Dispose();
        }
    }
}