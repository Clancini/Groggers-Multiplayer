using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Groggers.Multiplayer
{
    public ref struct MessageWriter
    {
        Span<byte> _backingBuffer;
        int _position;

        public MessageWriter(Span<byte> backingBuffer)
        {
            _backingBuffer = backingBuffer;
            _position = 0;
        }

        public void Write<T>(T value) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();

            MemoryMarshal.Write(_backingBuffer.Slice(_position, size), ref value);
            _position += size;
        }

        public void Write(string value)
        {
            // With one call we can both write the bytes of the string and get the amount of bytes written
            // We need to leave space for the length before it though
            int bytesWritten = Encoding.UTF8.GetBytes(value, _backingBuffer.Slice(_position + sizeof(int)));

            Write(bytesWritten);

            _position += bytesWritten;
        }
    }
}