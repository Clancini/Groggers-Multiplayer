using Groggers.Utils;
using Steamworks;
using System;

namespace Groggers.Multiplayer.Steam
{
    internal sealed class ClientMatchmaker
    {
        Callback<GameLobbyJoinRequested_t> _lobbyJoinRequest;
        Callback<LobbyEnter_t> _lobbyEntered;

        CSteamID _currentLobbyID;
        bool _isInLobby;

        public event Action<CSteamID> OnJoinedLobby;

        public ClientMatchmaker()
        {
            _lobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnRequestToJoinLobby);
            _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }

        void OnRequestToJoinLobby(GameLobbyJoinRequested_t callback)
        {
            if (_isInLobby)
            {
                Logger.Error("Can't join a new Steam lobby while already in one.");
                return;
            }

            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

            Logger.Info($"Trying to join the Steam lobby... ID: {callback.m_steamIDLobby}");
        }

        void OnLobbyEntered(LobbyEnter_t callback)
        {
            switch(callback.m_EChatRoomEnterResponse)
            {
                case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess:

                    _currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
                    _isInLobby = true;

                    OnJoinedLobby?.Invoke(_currentLobbyID);

                    Logger.Info($"Successfully joined the Steam lobby. ID: {callback.m_ulSteamIDLobby}");

                    break;

                case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseError:

                    Logger.Error($"Failed to join the Steam lobby. ID:  {callback.m_ulSteamIDLobby}");

                    break;
            }
        }

        public void LeaveLobby()
        {
            if (!_isInLobby) return;

            SteamMatchmaking.LeaveLobby(_currentLobbyID);
            _currentLobbyID = CSteamID.Nil;
            _isInLobby = false;

            Logger.Info("Left the Steam lobby.");
        }

        public void Dispose()
        {
            LeaveLobby();

            OnJoinedLobby = null;

            _lobbyJoinRequest.Dispose();
            _lobbyEntered.Dispose();
        }
    }
}