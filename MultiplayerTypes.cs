using System;

namespace Groggers.Multiplayer
{
    public delegate void MessageListener(in MessageReader message);

    public static class CommonValues
    {
        public const int HeaderSize = sizeof(int);
    }

    public enum Reliability
    {
        Reliable,
        Unreliable
    }

    /// <summary>
    /// Decides whether the client automatically tries to connect to the lobby's owner<br></br>
    /// right after joining one or waits for the game to explicitly tell it to connect
    /// </summary>
    public enum ConnectionMode
    {
        Auto,
        Manual
    }
}