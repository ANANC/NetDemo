using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ServerNetter : SimpleNetter
{
    public const int CLIENT_MAX = 100;  //连接客户端的上限

    private List<Socket> m_ReceiveSocketList = new List<Socket>();
    private Thread m_ListenThread;

    public ServerNetter(int port) :base()
    {
        m_OnAcceptCallback = AcceptClient;

        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Socket.Bind(new IPEndPoint(IPAddress.Any, port));
        m_Socket.Listen(CLIENT_MAX);

        m_ListenThread = new Thread(ReceiveClient);
        m_ListenThread.Start(); //开启线程
        m_ListenThread.IsBackground = true; //后台运行
    }

    public new void Update()
    {
        base.Update();
    }

    public new void Close()
    {
        base.Close();
        m_ListenThread.Abort();
        m_ListenThread.IsBackground = true;//关闭线程
    }

    private void ReceiveClient()
    {
        while (true)
        {
            BeginAccept();  
            Thread.Sleep(1000);//每隔1s检测 有没有连接我            
        }
    }

    private void AcceptClient(IAsyncResult ar)
    {
        Socket socket = m_Socket.EndAccept(ar);
        m_ReceiveSocketList.Add(socket);
    }
}
