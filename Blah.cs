using Groggers.Multiplayer;
using Groggers.Multiplayer.Steam;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.UI;

public class Blah : MonoBehaviour
{
    public readonly struct BlahMessage : IMessage
    {
        public readonly int Int;

        public BlahMessage(int i)
        {
            Int = i;
        }

        public BlahMessage(in MessageReader reader)
        {
            Int = reader.Read<int>(0, out _);
        }

        public int GetSize()
        {
            return sizeof(int);
        }

        public void SerializeWith(ref MessageWriter writer)
        {
            writer.Write<int>(Int);
        }
    }

    [SerializeField] Button _button;

    ServerManager _server;
    ClientManager _client;

    void Start()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        Debug.Log("Button clicked");

        if (CurrentPlayer.IsMainEditor)
        {
            _server = new ServerManager();
            _server.Start();

            _client = new ClientManager(ConnectionMode.Auto);

            ClientManager client2 = new ClientManager(ConnectionMode.Auto);

            _server.RegisterListener(0, OnMessageReceived);
            _client.RegisterListener(0, OnMessageReceived);

            BlahMessage message = new BlahMessage(42);
            _client.QueueMessage(0, Reliability.Reliable, in message);
            _server.QueueMessage(0, 0, Reliability.Reliable, in message);

        }
        else
        {

        }
    }

    void Update()
    {
        if (CurrentPlayer.IsMainEditor)
        {
            _server?.Update();
        }
        
        _client?.Update();
    }

    void OnMessageReceived(in MessageReader message)
    {
        Debug.Log("Message received: " + message.Type);
    }
}