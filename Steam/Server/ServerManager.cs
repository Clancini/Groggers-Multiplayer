
using Steamworks;

namespace Groggers.Multiplayer.Steam
{
    public sealed class ServerManager
    {
        ServerMatchmaker _matchmaker;
        ServerConnection _connection;

        PlayerSlot[] _playerSlots;

        public ServerManager()
        {
            _playerSlots = new PlayerSlot[MultiplayerSettings.MaxPlayers];

            _matchmaker = new ServerMatchmaker();
            _connection = new ServerConnection(_playerSlots);

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

        public HSteamNetConnection CreateLoopback()
        {
            return _connection.CreateLoopback();
        }

        public void Dispose()
        {
            _matchmaker.OnLobbyCreated -= OnLobbyCreated;

            _matchmaker.Dispose();
            _connection.Dispose();
        }
    }
}
