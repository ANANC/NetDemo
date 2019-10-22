using UnityEngine;
using Msg.C2G;
public class NetManager : MonoBehaviour
{
    public delegate void ReceiveStrCallback(string content);

    private readonly string HOST = "127.0.0.1";
    private readonly int PORT = 8220;
    private readonly byte CHECKINGCODE = 123;

    private Client m_Client;

    void Start()
    {
        Server.Instance.Port = PORT;
        Server.Instance.CheckingCode = CHECKINGCODE;
        Server.Instance.Start();

        m_Client = new Client();
        m_Client.CheckingCode = CHECKINGCODE;
        m_Client.Start();
    }

    void Update()
    {
        Server.Instance.Update();
        m_Client.Update();
    }

    private void OnDestroy()
    {
        m_Client.Destroy();
        Server.Instance.Destroy();
    }

    public void ClientContent()
    {
        m_Client.Content(HOST, PORT);
    }

    public void ClientSend(string message)
    {
        MESSAGE pack = new MESSAGE();
        pack.Content = message;
        m_Client.Send<MESSAGE>((int)CMD.Message, pack);
    }
}
