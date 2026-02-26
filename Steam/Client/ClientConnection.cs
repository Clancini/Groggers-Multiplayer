using Groggers.Utils;
using Steamworks;
using System;

namespace Groggers.Multiplayer.Steam
{
    internal sealed class ClientConnection
    {
        Callback<SteamNetConnectionStatusChangedCallback_t> _connectionStatusChanged;

        HSteamNetConnection _currentConnection;

        public event Action<HSteamNetConnection> OnConnectionChanged;

        public ClientConnection()
        {
            _connectionStatusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
        }

        public void ConnectIdentity(SteamNetworkingIdentity hostIdentity)
        {
            CloseConnection();

            _currentConnection = SteamNetworkingSockets.ConnectP2P(ref hostIdentity, 0, 0, null);

            Logger.Info($"Trying to connect to the host via Steam identity... Host ID: {hostIdentity.GetSteamID()}, Connection ID: {_currentConnection}");
        }

        public void ConnectIP(SteamNetworkingIPAddr hostAddress)
        {
            CloseConnection();

            _currentConnection = SteamNetworkingSockets.ConnectByIPAddress(ref hostAddress, 0, null);

            Logger.Info($"Trying to connect to the host via Steam IP... Host IP: {hostAddress.GetIPv4()} : {hostAddress.m_port}, Connection ID: {_currentConnection}");
        }

        // For loopback
        public void ConnectLoopback(HSteamNetConnection connection)
        {
            CloseConnection();

            _currentConnection = connection;

            OnConnectionChanged?.Invoke(_currentConnection);

            Logger.Info($"Connected to the host via loopback. Connection ID: {_currentConnection}");
        }

        void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
        {
            if (callback.m_hConn != _currentConnection) return;

            switch (callback.m_info.m_eState)
            {
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:

                    OnConnectionChanged?.Invoke(callback.m_hConn);

                    Logger.Info($"Successfully connected to the host. Connection ID: {callback.m_hConn}");

                    break;

                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:

                    CloseConnection();

                    break;

                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:

                    CloseConnection();

                    break;
            }
        }

        public void CloseConnection()
        {
            if (_currentConnection == HSteamNetConnection.Invalid) return;

            SteamNetworkingSockets.CloseConnection(_currentConnection, 0, null, false);

            _currentConnection = HSteamNetConnection.Invalid;

            OnConnectionChanged?.Invoke(HSteamNetConnection.Invalid);

            Logger.Info($"Disconnected from the host.");
        }

        public void Dispose()
        {
            CloseConnection();

            OnConnectionChanged = null;

            _connectionStatusChanged.Dispose();
        }
    }
}