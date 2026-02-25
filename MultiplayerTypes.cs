using System;

namespace Groggers.Multiplayer
{
    public delegate Action MessageListener(in MessageReader message);

    public static class CommonValues
    {
        public const int HeaderSize = sizeof(int) * 3;
    }

    public enum Reliability
    {
        Reliable,
        Unreliable
    }
}