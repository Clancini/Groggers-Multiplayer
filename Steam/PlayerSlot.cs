#if !DISABLESTEAMWORKS

using Steamworks;

namespace Groggers.Multiplayer.Steam
{
    public struct PlayerSlot
    {
        public HSteamNetConnection Connection { get; private set; }

        public void SetConnection(HSteamNetConnection connection)
        {
            Connection = connection;
        }
    }
}

#endif