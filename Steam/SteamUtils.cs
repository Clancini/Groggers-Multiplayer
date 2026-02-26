using Steamworks;
using System;

namespace Groggers.Multiplayer.Steam
{
    public static class SteamUtils
    {
        const int k_nSteamNetworkingSend_UnreliableNoDelay = 0 | 1;
        const int k_nSteamNetworkingSend_ReliableNoDelay = 8 | 1;

        // https://partner.steamgames.com/doc/api/steamnetworkingtypes#SteamNetworkingMessage_t
        public static int GetReliabilityFromEnum(Reliability reliability)
        {
            switch(reliability)
            {
                case Reliability.Reliable:
                    return k_nSteamNetworkingSend_ReliableNoDelay;
                case Reliability.Unreliable:
                    return k_nSteamNetworkingSend_UnreliableNoDelay;
            }

            return k_nSteamNetworkingSend_ReliableNoDelay;
        }

        public static IntPtr CreateMessage<T>(int messageType, Reliability reliability, in T message, HSteamNetConnection connection) where T : struct, IMessage
        {
            int messageSize = message.GetSize() + CommonValues.HeaderSize;

            IntPtr newMessage = SteamNetworkingUtils.AllocateMessage(messageSize);

            unsafe
            {
                SteamNetworkingMessage_t* messagePointer = (SteamNetworkingMessage_t*)newMessage.ToPointer();

                void* dataPointer = messagePointer->m_pData.ToPointer();

                messagePointer->m_conn = connection;
                messagePointer->m_nFlags = GetReliabilityFromEnum(reliability);

                Span<byte> span = new Span<byte>(dataPointer, messageSize);
                MessageWriter writer = new MessageWriter(span);

                writer.Write(messageType);

                message.SerializeWith(ref writer);
            }

            return newMessage;
        }

        unsafe public static MessageReader GetReader(IntPtr messagePtr)
        {
            SteamNetworkingMessage_t* messagePointer = (SteamNetworkingMessage_t*)messagePtr.ToPointer();
            void* dataPointer = messagePointer->m_pData.ToPointer();
            int messageSize = messagePointer->m_cbSize;

            ReadOnlySpan<byte> messageData = new ReadOnlySpan<byte>(dataPointer, messageSize);
            MessageReader reader = new MessageReader(messageData);

            return reader;
        }
    }
}