#if !DISABLESTEAMWORKS

using Steamworks;

namespace Groggers.Multiplayer.Steam
{
    public sealed class ClientManager
    {
        readonly ClientMatchmaker _matchmaker;
        readonly ClientConnection _connection;
        readonly ClientDispatcher _dispatcher;

        ConnectionMode _connectionMode;

        public ClientManager(ConnectionMode mode)
        {
            _matchmaker = new ClientMatchmaker();
            _connection = new ClientConnection();
            _dispatcher = new ClientDispatcher();

            _connectionMode = mode;
            if (_connectionMode == ConnectionMode.Auto)
                _matchmaker.OnJoinedLobby += OnJoinedLobby;

            _connection.OnConnectionChanged += OnConnected;
        }

        public void SetConnectionMode(ConnectionMode mode)
        {
            if (mode == _connectionMode) return;

            if (_connectionMode == ConnectionMode.Auto && mode == ConnectionMode.Manual)
                _matchmaker.OnJoinedLobby -= OnJoinedLobby;
            else if (_connectionMode == ConnectionMode.Manual && mode == ConnectionMode.Auto)
                _matchmaker.OnJoinedLobby += OnJoinedLobby;
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

        public void ConnectLoopback(uint connection)
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

        public void CloseConnection()
        {
            _connection.CloseConnection();
        }

        public void SetCanLeaveLobby(bool value)
        {
            _matchmaker.SetCanLeaveLobby(value);
        }

        public void LeaveLobby()
        {
            _matchmaker.LeaveLobby();
        }

        public void RegisterListener(int messageType, MessageListener listener)
        {
            _dispatcher.RegisterListener(messageType, listener);
        }

        public void UnregisterListener(int messageType, MessageListener listener)
        {
            _dispatcher.UnregisterListener(messageType, listener);
        }

        public void QueueMessage<T>(int messageType, Reliability reliability, in T message) where T : struct, IMessage
        {
            _dispatcher.QueueMessage(messageType, reliability, in message);
        }

        public void Dispose()
        {
            _matchmaker.OnJoinedLobby -= OnJoinedLobby;
            _connection.OnConnectionChanged -= OnConnected;

            _matchmaker.Dispose();
            _connection.Dispose();
        }
    }
}

#endif