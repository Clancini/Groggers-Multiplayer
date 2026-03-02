#if !DISABLESTEAMWORKS

using Groggers.Utils;
using Steamworks;

namespace Groggers.Multiplayer.Steam
{
    internal sealed class ServerConnection
    {
        HSteamListenSocket _listenSocketP2P;
        HSteamListenSocket _listenSocketIP;
        HSteamNetPollGroup _pollGroup;

        Callback<SteamNetConnectionStatusChangedCallback_t> _connectionStatusChanged;

        PlayerSlot[] _playerSlots;

        public ServerConnection(HSteamNetPollGroup pollGroup, PlayerSlot[] playerSlots)
        {
            _pollGroup = pollGroup;
            _playerSlots = playerSlots;

            _connectionStatusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
        }

        public void CreateSocketIdentity()
        {
            _listenSocketP2P = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);

            Logger.Info($"Created Steam identity listen socket");
        }

        public void CreateSocketIP()
        {
            SteamNetworkingIPAddr ipAddr = new SteamNetworkingIPAddr();
            ipAddr.Clear();
            ipAddr.m_port = MultiplayerSettings.IPPort;

            _listenSocketIP = SteamNetworkingSockets.CreateListenSocketIP(ref ipAddr, 0, null);

            Logger.Info($"Created Steam IP listen socket");
        }

        public HSteamNetConnection CreateLoopback()
        {
            SteamNetworkingIdentity server = new SteamNetworkingIdentity();
            SteamNetworkingIdentity client = new SteamNetworkingIdentity();

            SteamNetworkingSockets.CreateSocketPair(out HSteamNetConnection serverConnection, out HSteamNetConnection clientConnection, true, ref server, ref client);

            OnConnectionConnected(serverConnection);

            Logger.Info($"Created loopback connection. Server connection: {serverConnection}, Client connection: {clientConnection}");

            return clientConnection;
        }

        void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
        {
            if (callback.m_info.m_hListenSocket != _listenSocketP2P && callback.m_info.m_hListenSocket != _listenSocketIP)
                return;

            switch (callback.m_info.m_eState)
            {
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:

                    OnIncomingConnection(callback.m_hConn);

                    break;

                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:

                    OnConnectionConnected(callback.m_hConn);

                    break;

                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:

                    CloseConnection(callback.m_hConn);

                    break;

                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:

                    CloseConnection(callback.m_hConn);

                    break;
            }
        }

        void OnIncomingConnection(HSteamNetConnection connection)
        {
            Logger.Info($"New connection is connecting... Connection: {connection}");

            if (ServerManager.IndexOfFirstEmptySlot(_playerSlots) == -1)
            {
                Logger.Info($"Connection rejected because there are not slots available. Connection: {connection}");

                CloseConnection(connection);

                return;
            }

            SteamNetworkingSockets.AcceptConnection(connection);

            Logger.Info($"Accepted new connection. Connection: {connection}");
        }

        void OnConnectionConnected(HSteamNetConnection connection)
        {
            SteamNetworkingSockets.SetConnectionPollGroup(connection, _pollGroup);

            int slotIndex = ServerManager.IndexOfFirstEmptySlot(_playerSlots);
            PlayerSlot newPlayerSlot = new PlayerSlot();
            newPlayerSlot.SetConnection(connection);
            _playerSlots[slotIndex] = newPlayerSlot;

            Logger.Info($"New connection established. Slot: {slotIndex}, Connection: {connection}");
        }

        void CloseConnection(HSteamNetConnection connection)
        {
            SteamNetworkingSockets.CloseConnection(connection, 0, null, false);
            SteamNetworkingSockets.SetConnectionPollGroup(connection, HSteamNetPollGroup.Invalid);

            int slotIndex = ServerManager.IndexOfConnectionSlot(_playerSlots, connection);
            // We might just be cleaning up a connection that was rejected during connection, so there might not be a slot that contains it
            if (slotIndex != -1)
            {
                _playerSlots[slotIndex].SetConnection(HSteamNetConnection.Invalid);
            }

            Logger.Info($"Connection closed. Slot: {slotIndex}, Connection: {connection}");
        }

        public void Dispose()
        {
            for (int i = 0; i < _playerSlots.Length; i++)
            {
                if (_playerSlots[i].Connection != HSteamNetConnection.Invalid)
                {
                    CloseConnection(_playerSlots[i].Connection);
                }
            }

            SteamNetworkingSockets.CloseListenSocket(_listenSocketP2P);
            SteamNetworkingSockets.CloseListenSocket(_listenSocketIP);

            Logger.Info($"Closed Steam listen socket");
        }
    }
}

#endif