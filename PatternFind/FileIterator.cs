using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace PatternFind
{
    class FileIterator : IDisposable
    {
        private StreamReader streamReader { get; set; }
        bool isDisposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        public FileIterator(StreamReader streamReader)
        {
            this.streamReader = streamReader;
        }

        public FileIterator(Stream fileStream)
        {
            // TODO: Complete member initialization
            this.streamReader = new StreamReader(fileStream);
        }

        /// <summary>
        /// Iterates through the stream that was intitated and reads it line by line
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ReadLines()
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
                yield return line;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                streamReader.Dispose();
            }

            isDisposed = true;
        }
    }
}
