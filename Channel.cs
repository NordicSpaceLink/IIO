// Copyright (C) 2024 - Nordic Space Link
using System;
using System.Collections.Generic;

namespace NordicSpaceLink.IIO
{
    public class Channel
    {
        private readonly (string, bool)[] labels;
        private readonly IntPtr chn;

        public Context Context { get; }
        public Device Device { get; }

        public string Name { get; }
        public string ID { get; }
        public bool IsOutput { get; }
        public bool IsScanElement { get; }

        /// <summary>
        /// True if the channel is enabled
        /// </summary>
        public bool Enabled
        {
            get => NativeMethods.iio_channel_is_enabled(chn); set
            {
                if (value)
                    NativeMethods.iio_channel_enable(chn);
                else
                    NativeMethods.iio_channel_disable(chn);
            }
        }

        public ChannelType ChannelType => NativeMethods.iio_channel_get_type(chn);
        public Modifier Modifier => NativeMethods.iio_channel_get_modifier(chn);

        public Indexer<string, Attribute> Attributes { get; }

        internal (string, bool)[] GetLabels()
        {
            return labels;
        }

        internal unsafe Channel(Device device, IntPtr chn, string name, string id)
        {
            this.chn = chn;

            Context = device.Context;
            Device = device;

            Name = name;
            ID = id;

            IsOutput = NativeMethods.iio_channel_is_output(chn);
            IsScanElement = NativeMethods.iio_channel_is_scan_element(chn);

            var result = new List<(string, bool)>();

            if (!string.IsNullOrWhiteSpace(Name))
                result.Add((Name, IsOutput));
            if (!string.IsNullOrWhiteSpace(ID))
                result.Add((ID, IsOutput));

            labels = result.ToArray();

            // Device attributes
            var attrs = new List<(Attribute, string[])>();

            var count = NativeMethods.iio_channel_get_attrs_count(chn);
            for (int i = 0; i < (int)count; i++)
            {
                var namePtr = NativeMethods.iio_channel_get_attr(chn, (nuint)i);

                if (namePtr != null)
                {
                    var attrName = Common.ConstCString(namePtr, 1024);
                    attrs.Add((new ChannelAttribute(chn, attrName), new string[] { attrName }));
                }
            }

            Attributes = new Indexer<string, Attribute>(attrs);
        }

        private class ChannelAttribute : Attribute
        {
            private readonly IntPtr chan;

            public ChannelAttribute(IntPtr chan, string name) : base(name)
            {
                this.chan = chan;
            }

            protected unsafe override string GetValue()
            {
                const int buffersize = 4096;
                byte* buffer = stackalloc byte[buffersize];

                var size = Common.CheckError(NativeMethods.iio_channel_attr_read(chan, Name, buffer, buffersize));

                return new Span<byte>(buffer, (int)size).CString();
            }

            protected override void SetValue(string value)
            {
                Common.CheckError(NativeMethods.iio_channel_attr_write(chan, Name, value));
            }
        }
    }
}