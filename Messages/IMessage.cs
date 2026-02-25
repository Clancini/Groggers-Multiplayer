namespace Groggers.Multiplayer
{
    public interface IMessage
    {
        int GetSize();
        void SerializeWith(ref MessageWriter writer);
    }
}