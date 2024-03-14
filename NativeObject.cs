// Copyright (C) 2024 - Nordic Space Link
using System;

namespace NordicSpaceLink.IIO
{
    public abstract class NativeObject : IDisposable
    {
        private bool disposedValue;

        protected abstract void DoDispose();
        protected abstract void Free();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DoDispose();
                }

                Free();
                disposedValue = true;
            }
        }

        ~NativeObject()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}