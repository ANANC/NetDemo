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

    public Action<ClientNetter> m_ClientRegisterCallback;

    private List<ClientNetter> m_ReceiveSocketList = new List<ClientNetter>();
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
        //base.Update();
        //for (int index = 0; index < m_ReceiveSocketList.Count; index++)
        //{
        //    ServerClientTCPConnection clientConnection = new ServerClientTCPConnection();
        //    clientConnection.Connect(m_ReceiveSocketList[index], m_CheckingCode);
        //    NetBodyRegisterReceiver(clientConnection);
        //    clientConnection.m_Id = m_Clients.Count.ToString();
        //    m_Clients.Add(clientConnection);

        //    NetTestMgr.ShowStrContentEvent(true, "连接客户端");
        //}
        //m_ReceiveSocketList.Clear();

        //for (int index = 0; index < m_Clients.Count; index++)
        //{
        //    m_Clients[index].Update();
        //}
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
        ClientNetter clientNetter = new ClientNetter(socket);
        clientNetter.Id = m_ReceiveSocketList.Count;
        if (m_ClientRegisterCallback!=null)
        {
            m_ClientRegisterCallback(clientNetter);
        }

        m_ReceiveSocketList.Add(clientNetter);

        clientNetter.BeginReceive();
    }


}
