using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ClientNetter : SimpleNetter
{
    private int m_Id;
    public int Id
    {
        get { return m_Id; }
        set { m_Id = value; }
    }

    public ClientNetter() : base()
    {
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Socket.SendTimeout = 1000;
        m_Socket.ReceiveTimeout = 1000;
        m_Socket.NoDelay = true;

        m_OnConnectCallback = ConnectServer;
    }

    public ClientNetter(Socket socket) : base()
    {
        m_Socket = socket;

        m_OnConnectCallback = ConnectServer;
    }

    private void ConnectServer(IAsyncResult asr)
    {
        m_Socket.EndConnect(asr);
    }

}
