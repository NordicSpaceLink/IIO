// Copyright (C) 2024 - Nordic Space Link
using System;
using System.Collections.Generic;

namespace NordicSpaceLink.IIO
{
    public struct LibraryVersion
    {
        public readonly int Major;
        public readonly int Minor;
        public readonly string GitTag;

        public LibraryVersion(int major, int minor, string gitTag) : this()
        {
            Major = major;
            Minor = minor;
            GitTag = gitTag;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}-{GitTag}";
        }
    }

    public static class Library
    {
        private static readonly List<string> backends = new();

        /// <summary>
        /// Get the version of the libiio library
        /// </summary>
        public static LibraryVersion Version { get; }

        /// <summary>
        /// List of supported IIO backends
        /// </summary>
        public static IEnumerable<string> Backends { get => backends; }

        unsafe static Library()
        {
            nuint major, minor;
            Span<byte> git_tag = stackalloc byte[8];

            fixed (byte* git_tag_ptr = git_tag)
                NativeMethods.iio_library_get_version(&major, &minor, git_tag_ptr);

            Version = new LibraryVersion((int)major, (int)minor, git_tag.CString());

            var count = NativeMethods.iio_get_backends_count();
            for (int i = 0; i < (int)count; i++)
            {
                var name = Common.ConstCString(NativeMethods.iio_get_backend((nuint)i), 1024);
                if (!string.IsNullOrWhiteSpace(name))
                    backends.Add(name);
            }
        }

        public static bool HasBackend(string backend)
        {
            return NativeMethods.iio_has_backend(backend);
        }
    }
}