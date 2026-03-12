#if !DISABLESTEAMWORKS

using Steamworks;

namespace Groggers.Multiplayer.Steam
{
    public sealed class ServerManager
    {
        ServerMatchmaker _matchmaker;
        ServerConnection _connection;
        ServerDispatcher _dispatcher;

        HSteamNetPollGroup _pollGroup;
        PlayerSlot[] _playerSlots;

        public ServerManager()
        {
            _pollGroup = SteamNetworkingSockets.CreatePollGroup();
            _playerSlots = new PlayerSlot[MultiplayerSettings.MaxPlayers];

            _matchmaker = new ServerMatchmaker();
            _connection = new ServerConnection(_pollGroup, _playerSlots);
            _dispatcher = new ServerDispatcher(_pollGroup, _playerSlots);

            _matchmaker.OnLobbyCreated += OnLobbyCreated;
        }

        public void Start()
        {
            _matchmaker.CreateLobby();
        }

        void OnLobbyCreated()
        {
            _connection.CreateSocketIdentity();
            _connection.CreateSocketIP();
        }

        public uint CreateLoopback()
        {
            return _connection.CreateLoopback().m_HSteamNetConnection;
        }

        public void Update()
        {
            _dispatcher.Update();
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
            SteamNetworkingSockets.DestroyPollGroup(_pollGroup);

            _matchmaker.OnLobbyCreated -= OnLobbyCreated;

            _matchmaker.Dispose();
            _connection.Dispose();
        }

        #region Utils
        internal static int IndexOfFirstEmptySlot(PlayerSlot[] slots)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Connection == HSteamNetConnection.Invalid) return i;
            }

            return -1;
        }

        internal static int IndexOfConnectionSlot(PlayerSlot[] slots, HSteamNetConnection connection)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Connection == connection) return i;
            }

            return -1;
        }
        #endregion
    }
}

#endif