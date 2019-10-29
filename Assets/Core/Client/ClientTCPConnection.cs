using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ClientTCPConnection : BaseConnection
{
    private bool m_IsIPV4;                          //是否使用ipv4
    public bool IsIPV4
    {
        set
        {
            m_IsIPV4 = value;
        }
        get
        {
            return m_IsIPV4;
        }
    }

    private byte m_CheckingCode;
    public byte CheckingCode
    {
        set
        {
            m_CheckingCode = value;
        }
    }


    public void Connect(string host, int port)
    {
        Debug.Log("TCPConnection : " + host + "," + port);
        m_NetBuffer.Reset();

        IPAddress addr = null;
        Socket socket;
        if (m_IsIPV4)
        {
            // IPV4连接
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        else
        {
            // IPV6连接
            addr = NetUtils.GetIpAddress(host);
            Debug.LogWarning(string.Format(">>>>>>>>> ip = {0}, address family = {1} ", addr.ToString(), addr.AddressFamily));
            socket = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        socket.SendTimeout = 1000;
        socket.ReceiveTimeout = 1000;
        socket.NoDelay = true;

        ClientNetBody body = new ClientNetBody();

        NetPackage netPackage = new NetPackage();
        netPackage.SetNetBody(body);
        netPackage.checkingCode = m_CheckingCode;

        SetNetPackage(netPackage);
        SetSocket(socket);

        socket.BeginConnect(host, port, OnConnect, null);

    }

    private void OnConnect(IAsyncResult asr)
    {
        m_Socket.EndConnect(asr);
        NetTestMgr.ShowStrContentEvent(false, "客户端连接成功");
    }
}
