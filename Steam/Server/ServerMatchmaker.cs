#if !DISABLESTEAMWORKS

using Groggers.Utils;
using Steamworks;
using System;

namespace Groggers.Multiplayer.Steam
{
    internal sealed class ServerMatchmaker
    {
        Callback<LobbyCreated_t> _lobbyCreated;
        Callback<LobbyChatUpdate_t> _lobbyUpdated;

        CSteamID _lobbyID;
        bool _isInLobby;

        public event Action OnLobbyCreated;

        public ServerMatchmaker()
        {
            _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreationResult);
            _lobbyUpdated = Callback<LobbyChatUpdate_t>.Create(OnLobbyUpdate);
        }

        public void CreateLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MultiplayerSettings.MaxPlayers);

            Log.Info("Creating lobby...");
        }

        void OnLobbyCreationResult(LobbyCreated_t callback)
        {
            switch(callback.m_eResult)
            {
                case EResult.k_EResultOK:

                    Log.Info($"Lobby created successfully. ID: {callback.m_ulSteamIDLobby}");

                    _lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
                    _isInLobby = true;

                    OnLobbyCreated?.Invoke();

                    break;

                default:

                    Log.Error($"Failed to create lobby. Result: {callback.m_eResult}");
                    
                    break;
            }
        }

        void OnLobbyUpdate(LobbyChatUpdate_t callback)
        {
            CSteamID user = new CSteamID(callback.m_ulSteamIDUserChanged);

            if ((callback.m_rgfChatMemberStateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft) != 0)
            {
                if (user == SteamUser.GetSteamID())
                {
                    OnLeftLobby();
                }
            }
        }

        void OnLeftLobby()
        {
            _lobbyID = CSteamID.Nil;
            _isInLobby = false;

            Log.Info("Left the Steam lobby.");
        }

        void LeaveLobby()
        {
            if (!_isInLobby) return;

            SteamMatchmaking.LeaveLobby(_lobbyID);
        }

        public void Dispose()
        {
            OnLobbyCreated = null;

            LeaveLobby();

            _lobbyCreated.Dispose();
            _lobbyUpdated.Dispose();
        }
    }
}

#endif