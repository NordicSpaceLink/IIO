// Copyright (C) 2024 - Nordic Space Link

// This file is largely a manual conversion of iio.h with the following header:

/*
 * libiio - Library for interfacing industrial I/O (IIO) devices
 *
 * Copyright (C) 2014 Analog Devices, Inc.
 * Author: Paul Cercueil <paul.cercueil@analog.com>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * */

using System;
using System.Runtime.InteropServices;

namespace NordicSpaceLink.IIO
{
    public enum ChannelType
    {
        IIO_VOLTAGE,
        IIO_CURRENT,
        IIO_POWER,
        IIO_ACCEL,
        IIO_ANGL_VEL,
        IIO_MAGN,
        IIO_LIGHT,
        IIO_INTENSITY,
        IIO_PROXIMITY,
        IIO_TEMP,
        IIO_INCLI,
        IIO_ROT,
        IIO_ANGL,
        IIO_TIMESTAMP,
        IIO_CAPACITANCE,
        IIO_ALTVOLTAGE,
        IIO_CCT,
        IIO_PRESSURE,
        IIO_HUMIDITYRELATIVE,
        IIO_ACTIVITY,
        IIO_STEPS,
        IIO_ENERGY,
        IIO_DISTANCE,
        IIO_VELOCITY,
        IIO_CONCENTRATION,
        IIO_RESISTANCE,
        IIO_PH,
        IIO_UVINDEX,
        IIO_ELECTRICALCONDUCTIVITY,
        IIO_COUNT,
        IIO_INDEX,
        IIO_GRAVITY,
        IIO_CHAN_TYPE_UNKNOWN = int.MaxValue
    };

    public enum Modifier
    {
        NO_MOD,
        MOD_X,
        MOD_Y,
        MOD_Z,
        MOD_X_AND_Y,
        MOD_X_AND_Z,
        MOD_Y_AND_Z,
        MOD_X_AND_Y_AND_Z,
        MOD_X_OR_Y,
        MOD_X_OR_Z,
        MOD_Y_OR_Z,
        MOD_X_OR_Y_OR_Z,
        MOD_LIGHT_BOTH,
        MOD_LIGHT_IR,
        MOD_ROOT_SUM_SQUARED_X_Y,
        MOD_SUM_SQUARED_X_Y_Z,
        MOD_LIGHT_CLEAR,
        MOD_LIGHT_RED,
        MOD_LIGHT_GREEN,
        MOD_LIGHT_BLUE,
        MOD_QUATERNION,
        MOD_TEMP_AMBIENT,
        MOD_TEMP_OBJECT,
        MOD_NORTH_MAGN,
        MOD_NORTH_TRUE,
        MOD_NORTH_MAGN_TILT_COMP,
        MOD_NORTH_TRUE_TILT_COMP,
        MOD_RUNNING,
        MOD_JOGGING,
        MOD_WALKING,
        MOD_STILL,
        MOD_ROOT_SUM_SQUARED_X_Y_Z,
        MOD_I,
        MOD_Q,
        MOD_CO2,
        MOD_VOC,
        MOD_LIGHT_UV,
    };

    internal unsafe static class NativeMethods
    {
        [DllImport("iio")]
        public static extern IntPtr iio_create_default_context();
        [DllImport("iio")]
        public static extern IntPtr iio_create_local_context();
        [DllImport("iio")]
        public static extern IntPtr iio_create_network_context(string host);
        [DllImport("iio")]
        public static extern IntPtr iio_create_context_from_uri(string uri);
        [DllImport("iio")]
        public static extern void iio_context_destroy(IntPtr ctx);

        [DllImport("iio")]
        public static extern nint iio_context_set_timeout(IntPtr ctx, nuint timeout_ms);

        [DllImport("iio")]
        public static extern void iio_library_get_version(nuint* major, nuint* minor, byte* git_tag);

        [DllImport("iio")]
        public static extern bool iio_has_backend(string backend);

        [DllImport("iio")]
        public static extern nuint iio_get_backends_count();

        [DllImport("iio")]
        public static extern byte* iio_get_backend(nuint index);

        [DllImport("iio")]
        public static extern void iio_strerror(nint err, byte* dst, nint len);

        [DllImport("iio")]
        public static extern nuint iio_context_get_attrs_count(IntPtr ctx);
        [DllImport("iio")]
        public static extern int iio_context_get_attr(IntPtr ctx, nuint index, byte** name, byte** value);

        [DllImport("iio")]
        public static extern IntPtr iio_context_find_device(IntPtr ctx, string name);
        [DllImport("iio")]
        public static extern nuint iio_context_get_devices_count(IntPtr ctx);
        [DllImport("iio")]
        public static extern IntPtr iio_context_get_device(IntPtr ctx, nuint index);


        [DllImport("iio")]
        public static extern int iio_device_set_kernel_buffers_count(IntPtr dev, nuint nb_buffers);

        [DllImport("iio")]
        public static extern nuint iio_device_get_channels_count(IntPtr dev);
        [DllImport("iio")]
        public static extern nuint iio_device_get_attrs_count(IntPtr dev);
        [DllImport("iio")]
        public static extern nuint iio_device_get_buffer_attrs_count(IntPtr dev);
        [DllImport("iio")]
        public static extern IntPtr iio_device_get_channel(IntPtr dev, nuint index);
        [DllImport("iio")]
        public static extern byte* iio_device_get_attr(IntPtr dev, nuint index);
        [DllImport("iio")]
        public static extern byte* iio_device_get_buffer_attr(IntPtr dev, nuint index);

        [DllImport("iio")]
        public static extern IntPtr iio_device_find_channel(IntPtr dev, string name, bool output);
        [DllImport("iio")]
        public static extern nint iio_device_attr_read(IntPtr dev, string attr, byte* dst, nint len);
        [DllImport("iio")]
        public static extern nint iio_device_attr_write(IntPtr dev, string attr, string src);

        [DllImport("iio")]
        public static extern nint iio_device_buffer_attr_read(IntPtr dev, string attr, byte* dst, nuint len);
        [DllImport("iio")]
        public static extern nint iio_device_buffer_attr_write(IntPtr dev, string attr, string src);

        [DllImport("iio")]
        public static extern nuint iio_device_get_debug_attrs_count(IntPtr dev);
        [DllImport("iio")]
        public static extern byte* iio_device_get_debug_attr(IntPtr dev, nuint index);
        [DllImport("iio")]
        public static extern nint iio_device_debug_attr_read(IntPtr dev, string attr, byte* dst, nuint len);
        [DllImport("iio")]
        public static extern nint iio_device_debug_attr_write(IntPtr dev, string attr, string src);

        [DllImport("iio")]
        public static extern byte* iio_device_get_id(IntPtr dev);
        [DllImport("iio")]
        public static extern byte* iio_device_get_name(IntPtr dev);

        [DllImport("iio")]
        public static extern nint iio_device_get_sample_size(IntPtr dev);

        [DllImport("iio")]
        public static extern byte* iio_channel_get_id(IntPtr chn);
        [DllImport("iio")]
        public static extern byte* iio_channel_get_name(IntPtr chn);
        [DllImport("iio")]
        public static extern bool iio_channel_is_output(IntPtr chn);
        [DllImport("iio")]
        public static extern bool iio_channel_is_scan_element(IntPtr chn);
        [DllImport("iio")]
        public static extern nuint iio_channel_get_attrs_count(IntPtr chn);
        [DllImport("iio")]
        public static extern byte* iio_channel_get_attr(IntPtr chn, nuint index);
        [DllImport("iio")]
        public static extern nint iio_channel_attr_write(IntPtr chn, string attr, string src);
        [DllImport("iio")]
        public static extern nint iio_channel_attr_read(IntPtr dev, string attr, byte* dst, nuint len);

        [DllImport("iio")]
        public static extern void iio_channel_enable(IntPtr chn);
        [DllImport("iio")]
        public static extern void iio_channel_disable(IntPtr chn);
        [DllImport("iio")]
        public static extern bool iio_channel_is_enabled(IntPtr chn);

        [DllImport("iio")]
        public static extern ChannelType iio_channel_get_type(IntPtr chn);
        [DllImport("iio")]
        public static extern Modifier iio_channel_get_modifier(IntPtr chn);

        [DllImport("iio")]
        public static extern IntPtr iio_device_create_buffer(IntPtr dev, nuint samples_count, bool cyclic);
        [DllImport("iio")]
        public static extern void iio_buffer_destroy(IntPtr buf);
        [DllImport("iio")]
        public static extern nint iio_buffer_get_poll_fd(IntPtr buf);
        [DllImport("iio")]
        public static extern nint iio_buffer_set_blocking_mode(IntPtr buf, bool blocking);
        [DllImport("iio")]
        public static extern void iio_buffer_cancel(IntPtr buf);
        [DllImport("iio")]
        public static extern nint iio_buffer_refill(IntPtr buf);
        [DllImport("iio")]
        public static extern nint iio_buffer_push(IntPtr buf);
        [DllImport("iio")]
        public static extern nint iio_buffer_push_partial(IntPtr buf, nuint samples_count);
        [DllImport("iio")]
        public static extern void* iio_buffer_start(IntPtr buf);
        [DllImport("iio")]
        public static extern void* iio_buffer_end(IntPtr buf);
        [DllImport("iio")]
        public static extern long iio_buffer_step(IntPtr buf);
    }
}