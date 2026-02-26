using Groggers.Utils;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Groggers.Multiplayer.Steam
{
    internal sealed class ClientDispatcher
    {
        HSteamNetConnection _currentConnection;

        IntPtr[] _receivedMessages;

        IntPtr[] _messagesToSend;
        int _messagesToSendPosition;
        long[] _messagesSendResults;

        Dictionary<int, MessageListener> _listeners;

        public ClientDispatcher()
        {
            _receivedMessages = new IntPtr[MultiplayerSettings.ReceivedMessagesCapacity];
            _messagesToSend = new IntPtr[MultiplayerSettings.MessagesToSendCapacity];
            _messagesSendResults = new long[MultiplayerSettings.MessagesToSendCapacity];

            _listeners = new Dictionary<int, MessageListener>();
        }

        public void SetConnection(HSteamNetConnection connection)
        {
            _currentConnection = connection;
        }

        public void Update()
        {
            if (_currentConnection == HSteamNetConnection.Invalid) return;

            PollConnection();
            
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
        void PollConnection()
        {
            int receivedCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(_currentConnection, _receivedMessages, MultiplayerSettings.ReceivedMessagesCapacity);

            while (receivedCount > 0)
            {
                for (int i = 0; i < receivedCount; i++)
                {
                    ProcessMessage(i);
                }

                receivedCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(_currentConnection, _receivedMessages, MultiplayerSettings.ReceivedMessagesCapacity);
            }
        }

        unsafe void ProcessMessage(int messageIndex)
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
        public void QueueMessage<T>(int messageType, Reliability reliability, in T message) where T : struct, IMessage
        {
            // -1 as target because the client only has one connection, could have been any number
            IntPtr newMessage = SteamUtils.CreateMessage(messageType, reliability, in message, _currentConnection);

            if (_messagesToSendPosition >= MultiplayerSettings.MessagesToSendCapacity)
            {
                SendMessages();
            }

            _messagesToSend[_messagesToSendPosition] = newMessage;
            _messagesToSendPosition++;

            Logger.Info($"Queued message of type {messageType} for sending. Reliability: {reliability}. Current queue size: {_messagesToSendPosition}");
        }

        void SendMessages()
        {
            SteamNetworkingSockets.SendMessages(_messagesToSendPosition, _messagesToSend, _messagesSendResults);
            _messagesToSendPosition = 0;
        }
        #endregion
    }
}