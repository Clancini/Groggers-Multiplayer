using Groggers.Multiplayer;
using Groggers.Multiplayer.Steam;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.UI;

public class Blah : MonoBehaviour
{
    [SerializeField] Button _button;

    void Start()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        Debug.Log("Button clicked");

        if (CurrentPlayer.IsMainEditor)
        {
            ServerManager server = new ServerManager();
            server.Start();

            ClientManager client = new ClientManager(ConnectionMode.Manual);
            client.ConnectLoopback(server.CreateLoopback());
        }
        else
        {
            ClientManager client = new ClientManager(ConnectionMode.Manual);
            client.ConnectIP("127.0.0.1");
        }
    }
}