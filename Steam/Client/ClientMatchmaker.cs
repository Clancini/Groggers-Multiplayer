#if !DISABLESTEAMWORKS

using Groggers.Utils;
using Steamworks;
using System;

namespace Groggers.Multiplayer.Steam
{
    internal sealed class ClientMatchmaker
    {
        Callback<GameLobbyJoinRequested_t> _lobbyJoinRequest;
        Callback<LobbyEnter_t> _lobbyEntered;
        Callback<LobbyChatUpdate_t> _lobbyUpdate;

        CSteamID _currentLobbyID;
        bool _isInLobby;
        bool _canLeaveLobby;    // If the player is hosting, we can't let the client object leave the lobby. The lobby is owned by the host for as long as the server exists

        public event Action<CSteamID> OnJoinedLobby;

        public ClientMatchmaker()
        {
            _lobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnRequestToJoinLobby);
            _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            _lobbyUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyUpdate);
        }

        void OnRequestToJoinLobby(GameLobbyJoinRequested_t callback)
        {
            if (_isInLobby)
            {
                Log.Error("Can't join a new Steam lobby while already in one.");
                return;
            }

            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

            Log.Info($"Trying to join the Steam lobby... ID: {callback.m_steamIDLobby}");
        }

        void OnLobbyEntered(LobbyEnter_t callback)
        {
            switch(callback.m_EChatRoomEnterResponse)
            {
                case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess:

                    _currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
                    _isInLobby = true;

                    OnJoinedLobby?.Invoke(_currentLobbyID);

                    Log.Info($"Successfully joined the Steam lobby. ID: {callback.m_ulSteamIDLobby}");

                    break;

                case (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseError:

                    Log.Error($"Failed to join the Steam lobby. ID:  {callback.m_ulSteamIDLobby}");

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
            _currentLobbyID = CSteamID.Nil;
            _isInLobby = false;

            Log.Info("Left the Steam lobby.");
        }

        public void SetCanLeaveLobby(bool value)
        {
            _canLeaveLobby = value;
        }

        public void LeaveLobby()
        {
            if (!_isInLobby) return;
            if (_canLeaveLobby) Log.Warning("Can't leave lobby");

            SteamMatchmaking.LeaveLobby(_currentLobbyID);
        }

        public void Dispose()
        {
            if (_canLeaveLobby) LeaveLobby();

            OnJoinedLobby = null;

            _lobbyJoinRequest.Dispose();
            _lobbyEntered.Dispose();
            _lobbyUpdate.Dispose();
        }
    }
}

#endif