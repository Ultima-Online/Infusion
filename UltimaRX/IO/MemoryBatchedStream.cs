﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UltimaRX.IO
{
    public class MemoryBatchedStream : IPullStream
    {
        private IEnumerator<byte[]> contentEnumerator;
        private int position;

        public MemoryBatchedStream(IEnumerable<byte[]> content)
        {
            contentEnumerator = content.GetEnumerator();
            MoveNext();
        }

        private bool MoveNext()
        {
            if (contentEnumerator.MoveNext())
            {
                DataAvailable = true;
                if (contentEnumerator.Current.Length == 0)
                {
                    return MoveNext();
                }

                position = 0;
                return true;
            }

            position = -1;
            DataAvailable = false;
            return false;
        }

        public bool DataAvailable { get; private set; }

        public int ReadByte()
        {
            if (!DataAvailable)
                return -1;

            byte result = contentEnumerator.Current[position];

            IncrementPosition();

            return result;
        }

        private void IncrementPosition()
        {
            position++;
            if (position >= contentEnumerator.Current.Length)
                MoveNext();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < offset + count; i++)
            {
                int value = ReadByte();
                if (value < 0 || value > 255)
                {
                    return i - offset;
                }

                buffer[i] = (byte)value;
            }

            return count;
        }

        public void Dispose()
        {
            contentEnumerator?.Dispose();
        }
    }
}
