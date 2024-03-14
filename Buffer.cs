// Copyright (C) 2024 - Nordic Space Link
using System;

namespace NordicSpaceLink.IIO
{
    public class Buffer : NativeObject
    {
        private readonly Device device;
        private readonly IntPtr buf;
        private bool blocking = true;

        /// <summary>
        /// Get the step size between two samples of one channel
        /// </summary>
        public int Step => (int)NativeMethods.iio_buffer_step(buf);

        /// <summary>
        /// Make Refill() and Push() blocking or not. Defaults to true.
        /// </summary>
        public bool BlockingMode { get => blocking; set { Common.CheckError(NativeMethods.iio_buffer_set_blocking_mode(buf, value)); blocking = value; } }

        /// <summary>
        /// Get a pollable file descriptor
        /// 
        /// Can be used to know when iio_buffer_refill() or iio_buffer_push() can be called
        /// </summary>
        public nint PollFD => Common.CheckError(NativeMethods.iio_buffer_get_poll_fd(buf));

        /// <summary>
        /// Get a span pointing to the current buffer.
        /// </summary>
        /// <returns>A full span with the current buffer</returns>
        public unsafe Span<byte> GetBuffer()
        {
            var start = (byte*)NativeMethods.iio_buffer_start(buf);
            var end = (byte*)NativeMethods.iio_buffer_end(buf);

            var diff = end - start;

            return new Span<byte>(start, (int)diff);
        }

        internal Buffer(Device device, IntPtr buf)
        {
            this.device = device;
            this.buf = buf;

            if (buf == IntPtr.Zero)
                throw new Exception("Could not allocate buffer");
        }

        /// <summary>
        /// Cancel all buffer operations
        /// </summary>
        public void Cancel()
        {
            NativeMethods.iio_buffer_cancel(buf);
        }

        /// <summary>
        /// Try to fetch more samples from the hardware.
        /// </summary>
        /// <param name="bytesRead">Will return the number of bytes read</param>
        /// <returns>True if the refill call succeeded.</returns>
        public bool TryRefill(out int bytesRead)
        {
            bytesRead = 0;

            var result = NativeMethods.iio_buffer_refill(buf);
            if (result < 0)
                return false;
            
            bytesRead = (int)result;
            return true;
        }

        /// <summary>
        /// Fetch more samples from the hardware
        /// </summary>
        /// <returns>A span containing the bytes read</returns>
        public ReadOnlySpan<byte> Refill()
        {
            var read = (int)Common.CheckError(NativeMethods.iio_buffer_refill(buf));

            return GetBuffer()[..read];
        }

        /// <summary>
        /// Try to send the samples to the hardware
        /// </summary>
        /// <param name="pushedBytes">The number of bytes written</param>
        /// <returns>True if the push succeeded</returns>
        public bool TryPush(out int pushedBytes)
        {
            pushedBytes = 0;
            var result = (int)Common.CheckError(NativeMethods.iio_buffer_push(buf));
            if (result < 0)
                return false;

            pushedBytes = result;
            return true;
        }

        /// <summary>
        /// Send the samples to the hardware
        /// </summary>
        /// <returns>The number of bytes written</returns>
        public int Push()
        {
            return (int)Common.CheckError(NativeMethods.iio_buffer_push(buf));
        }

        /// <summary>
        /// Try to send a given number of samples to the hardware
        /// </summary>
        /// <param name="sample_count">The number of samples to submit</param>
        /// <param name="pushedBytes">The number of bytes written</param>
        /// <returns>True if the push succeeded</returns>
        public bool TryPushPartial(int sample_count, out int pushedBytes)
        {
            pushedBytes = 0;
            var result = (int)Common.CheckError(NativeMethods.iio_buffer_push_partial(buf, (nuint)sample_count));
            if (result < 0)
                return false;

            pushedBytes = result;
            return true;
        }

        /// <summary>
        /// Send a given number of samples to the hardware
        /// </summary>
        /// <param name="sample_count">The number of samples to submit</param>
        /// <returns>The number of bytes written</returns>
        public int PushPartial(int sample_count)
        {
            return (int)Common.CheckError(NativeMethods.iio_buffer_push_partial(buf, (nuint)sample_count));
        }

        protected override void DoDispose()
        {
        }

        protected override void Free()
        {
            NativeMethods.iio_buffer_destroy(buf);
            device.FreeBuffer(this);
        }
    }
}