using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Msg.C2G;
public class NetManager : MonoBehaviour
{
    public delegate void ReceiveStrCallback(string content);

    private readonly string HOST = "127.0.0.1";
    private readonly int PORT = 8220;
    private readonly byte CHECKINGCODE = 123;

    private Client m_Client;
    private Server m_Server;

    void Start()
    {
        m_Server = new Server();
        m_Server.Port = PORT;
        m_Server.CheckingCode = CHECKINGCODE;
        m_Server.Start();

        m_Client = new Client();
        m_Client.CheckingCode = CHECKINGCODE;
        m_Client.Start();
    }

    void Update()
    {
        m_Server.Update();
        m_Client.Update();
    }

    private void OnDestroy()
    {
        m_Client.Destroy();
        m_Server.Destroy();
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
