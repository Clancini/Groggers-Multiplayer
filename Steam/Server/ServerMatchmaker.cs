#if !DISABLESTEAMWORKS

using Groggers.Utils;
using Steamworks;
using System;

namespace Groggers.Multiplayer.Steam
{
    internal sealed class ServerMatchmaker
    {
        Callback<LobbyCreated_t> _lobbyCreated;

        public event Action OnLobbyCreated;

        public ServerMatchmaker()
        {
            _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreationResult);
        }

        public void CreateLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MultiplayerSettings.MaxPlayers);

            Logger.Info("Creating lobby...");
        }

        void OnLobbyCreationResult(LobbyCreated_t callback)
        {
            switch(callback.m_eResult)
            {
                case EResult.k_EResultOK:

                    Logger.Info($"Lobby created successfully. ID: {callback.m_ulSteamIDLobby}");

                    OnLobbyCreated?.Invoke();

                    break;

                default:

                    Logger.Error($"Failed to create lobby. Result: {callback.m_eResult}");
                    
                    break;
            }
        }

        public void Dispose()
        {
            OnLobbyCreated = null;

            _lobbyCreated.Dispose();
        }
    }
}

#endif