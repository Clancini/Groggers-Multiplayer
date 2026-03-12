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
            int readPosition = 0;

            Int = reader.Read<int>(ref readPosition);
        }

        public int GetSize()
        {
            return sizeof(int);
        }

        public void SerializeWith(ref MessageWriter writer)
        {
            writer.Write(Int);
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

            _client = new ClientManager(ConnectionMode.Manual);
            _client.ConnectLoopback(_server.CreateLoopback());

            _server.RegisterListener(0, OnMessageReceived);
            _client.RegisterListener(0, OnMessageReceived);

            BlahMessage message = new BlahMessage(42);
            _client.QueueMessage(0, Reliability.Reliable, in message);
            _server.QueueMessage(0, 0, Reliability.Reliable, in message);
        }
        else
        {
            _client = new ClientManager(ConnectionMode.Manual);
            _client.ConnectIP("127.0.0.1");
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