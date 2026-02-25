using Steamworks;
using System;

namespace Groggers.Multiplayer.Steam
{
    public sealed class ClientManager
    {
        readonly ClientMatchmaker _matchmaker;
        readonly ClientConnection _connection;
        readonly ClientDispatcher _dispatcher;

        public ClientManager(ConnectionMode mode)
        {
            _matchmaker = new ClientMatchmaker();
            _connection = new ClientConnection();
            _dispatcher = new ClientDispatcher();

            if (mode == ConnectionMode.Auto)
                _matchmaker.OnJoinedLobby += OnJoinedLobby;

            _connection.OnConnected += OnConnected;
        }

        void OnJoinedLobby(CSteamID lobbyID)
        {
            CSteamID hostID = SteamMatchmaking.GetLobbyOwner(lobbyID);

            SteamNetworkingIdentity hostIdentity = new SteamNetworkingIdentity();
            hostIdentity.SetSteamID(hostID);

            _connection.ConnectIdentity(hostIdentity);
        }

        void OnConnected(HSteamNetConnection connection)
        {
            _dispatcher.SetConnection(connection);
        }

        public void Update()
        {
            _dispatcher.Update();
        }

        public void ConnectLoopback(HSteamNetConnection connection)
        {
            _connection.ConnectLoopback(connection);
        }

        public void ConnectIP(string ipAddress)
        {
            SteamNetworkingIPAddr hostAddress = new SteamNetworkingIPAddr();
            hostAddress.ParseString(ipAddress);
            hostAddress.m_port = MultiplayerSettings.IPPort;

            _connection.ConnectIP(hostAddress);
        }

        public void ConnectIdentity(CSteamID hostID)
        {
            SteamNetworkingIdentity hostIdentity = new SteamNetworkingIdentity();
            hostIdentity.SetSteamID(hostID);

            _connection.ConnectIdentity(hostIdentity);
        }

        public void RegisterListener(int messageType, MessageListener listener)
        {
            _dispatcher.RegisterListener(messageType, listener);
        }

        public void UnregisterListener(int messageType, MessageListener listener)
        {
            _dispatcher.UnregisterListener(messageType, listener);
        }

        public void QueueMessage<T>(int messageType, int target, Reliability reliability, in T message) where T : struct, IMessage
        {
            _dispatcher.QueueMessage(messageType, target, reliability, in message);
        }

        public void Dispose()
        {
            _matchmaker.OnJoinedLobby -= OnJoinedLobby;
            _connection.OnConnected -= OnConnected;

            _matchmaker.Dispose();
            _connection.Dispose();
        }
    }
}