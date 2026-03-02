using System.Runtime.CompilerServices;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Groggers.Multiplayer
{
    public readonly ref struct MessageReader
    {
        public readonly int Type;

        readonly ReadOnlySpan<byte> _backingBuffer;

        public MessageReader(ReadOnlySpan<byte> backingBuffer)
        {
            _backingBuffer = CutMessageHeader(backingBuffer, out Type);
        }

        public T Read<T>(int position, out int newPosition) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();

            ReadOnlySpan<byte> slice = _backingBuffer.Slice(position, size);
            T value = MemoryMarshal.Read<T>(slice);

            newPosition = position + size;

            return value;
        }

        public string Read(int position, out int newPosition)
        {
            int length = Read<int>(position, out newPosition);

            position = newPosition;

            ReadOnlySpan<byte> slice = _backingBuffer.Slice(position, length);
            string value = Encoding.UTF8.GetString(slice);

            newPosition = position + length;

            return value;
        }

        public static ReadOnlySpan<byte> CutMessageHeader(ReadOnlySpan<byte> messageData, out int type)
        {
            ReadOnlySpan<byte> header = messageData.Slice(0, CommonValues.HeaderSize);

            type = MemoryMarshal.Read<int>(header);

            return messageData.Slice(CommonValues.HeaderSize);
        }
    }
}