// Copyright (C) 2024 - Nordic Space Link
using System;

namespace NordicSpaceLink.IIO
{
    internal static class Common
    {
        public unsafe static string ErrorDescription(nint error)
        {
            const int buffersize = 1024;
            Span<byte> buffer = stackalloc byte[buffersize];

            fixed (byte* bufferPtr = buffer)
                NativeMethods.iio_strerror(error, bufferPtr, buffersize);

            return buffer.CString();
        }

        public static nint CheckError(nint result, string message = "")
        {
            if (result < 0)
            {
                if (message == "")
                    throw new Exception($"Error: {ErrorDescription(-result)}");
                throw new Exception($"{message}. Error: {ErrorDescription(-result)}");
            }
            return result;
        }

        internal static int StrLen(Span<byte> str)
        {
            var idx = str.IndexOf((byte)0);
            if (idx < 0)
                return str.Length;
            return idx;
        }

        internal static string CString(this Span<byte> str)
        {
            var len = StrLen(str);
            return System.Text.Encoding.ASCII.GetString(str[..len]);
        }

        internal unsafe static string ConstCString(byte* ptr, int max_length)
        {
            if (ptr == null)
                return "";

            var str = new Span<byte>(ptr, max_length);

            var len = StrLen(str);
            return System.Text.Encoding.ASCII.GetString(str[..len]);
        }
    }
}