using Groggers.Utils;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Groggers.Multiplayer.Steam
{
    internal sealed class ServerDispatcher
    {
        IntPtr[] _receivedMessages;

        IntPtr[] _messagesToSend;
        int _messagesToSendPosition;
        long[] _messagesSendResults;

        Dictionary<int, MessageListener> _listeners;

        HSteamNetPollGroup _pollGroup;
        PlayerSlot[] _playerSlots;

        public ServerDispatcher(HSteamNetPollGroup pollGroup, PlayerSlot[] playerSlots)
        {
            _pollGroup = pollGroup;
            _playerSlots = playerSlots;

            _receivedMessages = new IntPtr[MultiplayerSettings.ReceivedMessagesCapacity];
            _messagesToSend = new IntPtr[MultiplayerSettings.MessagesToSendCapacity];
            _messagesSendResults = new long[MultiplayerSettings.MessagesToSendCapacity];

            _listeners = new Dictionary<int, MessageListener>();
        }

        public void Update()
        {
            PollConnections();

            if (_messagesToSendPosition > 0)
            {
                SendMessages();
            }
        }

        public void RegisterListener(int messageType, MessageListener listener)
        {
            if (_listeners.TryGetValue(messageType, out MessageListener existingListenerGroup))
            {
                existingListenerGroup += listener;
                _listeners[messageType] = existingListenerGroup;
            }
            else
            {
                _listeners.Add(messageType, listener);
            }

            Logger.Info($"Registered listener for message type {messageType}");
        }

        public void UnregisterListener(int messageType, MessageListener listener)
        {
            if (_listeners.TryGetValue(messageType, out MessageListener existingListenerGroup))
            {
                existingListenerGroup -= listener;
                _listeners[messageType] = existingListenerGroup;
            }

            if (existingListenerGroup == null)
            {
                _listeners.Remove(messageType);
            }
        }

        #region Receiving
        void PollConnections()
        {
            int receivedCount = SteamNetworkingSockets.ReceiveMessagesOnPollGroup(_pollGroup, _receivedMessages, _receivedMessages.Length);

            while (receivedCount > 0)
            {
                Logger.Info($"Received {receivedCount} messages. Starting processing...");

                for (int i = 0; i < receivedCount; i++)
                {
                    ProcessMessage(i);
                }

                receivedCount = SteamNetworkingSockets.ReceiveMessagesOnPollGroup(_pollGroup, _receivedMessages, _receivedMessages.Length);

                Logger.Info($"Finished. {receivedCount} more found");
            }
        }

        void ProcessMessage(int messageIndex)
        {
            IntPtr messageAtIndex = _receivedMessages[messageIndex];

            try
            {
                MessageReader reader = SteamUtils.GetReader(messageAtIndex);

                if (_listeners.TryGetValue(reader.Type, out MessageListener listenerGroup))
                {
                    listenerGroup.Invoke(reader);
                }
            }
            finally
            {
                SteamNetworkingMessage_t.Release(messageAtIndex);
            }
        }
        #endregion

        #region Sending
        public void QueueMessage<T>(int messageType, int target, Reliability reliability, in T message) where T : struct, IMessage
        {
            HSteamNetConnection targetConnection = _playerSlots[target].Connection;
            IntPtr newMessage = SteamUtils.CreateMessage(messageType, target, reliability, in message, targetConnection);

            if (_messagesToSendPosition >= MultiplayerSettings.MessagesToSendCapacity)
            {
                SendMessages();
            }

            _messagesToSend[_messagesToSendPosition] = newMessage;
            _messagesToSendPosition++;
        }

        void SendMessages()
        {
            SteamNetworkingSockets.SendMessages(_messagesToSendPosition, _messagesToSend, _messagesSendResults);
            _messagesToSendPosition = 0;
        }
        #endregion
    }
}