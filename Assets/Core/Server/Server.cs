using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using static NetManager;

public class Server
{
    private int m_Port;
    public int Port
    {
        set { m_Port = value; }
    }

    private byte m_CheckingCode;
    public byte CheckingCode
    {
        set
        {
            m_CheckingCode = value;
        }
    }

    private Socket m_Server;
    private List<ServerTCPConnection> m_Clients;
    private byte[] m_ClinetDatas;
    private Thread m_ListenThread;
    public ReceiveStrCallback ServerReceiveStrCallback;


    public void Start()
    {
        m_Clients = new List<ServerTCPConnection>();

        m_Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Server.Bind(new IPEndPoint(IPAddress.Any, m_Port));
        m_Server.Listen(100);

        m_ListenThread = new Thread(ReceiveClient);
        m_ListenThread.Start(); //开启线程
        m_ListenThread.IsBackground = true; //后台运行
    }

    public void Update()
    {
        for (int index = 0; index < m_Clients.Count; index++)
        {
            m_Clients[index].Update();
        }
    }


    public void Destroy()
    {
        OnApplicationQuit();
    }

    private void ReceiveClient()
    {
        while (true)
        {
            m_Server.BeginAccept(AcceptClient, null); //开始接受客户端连接请求            
            Thread.Sleep(1000);//每隔1s检测 有没有连接我            
        }
    }
    private void AcceptClient(IAsyncResult ar)
    {
        Socket socket = m_Server.EndAccept(ar);
        ServerTCPConnection clientConnection = new ServerTCPConnection();
        clientConnection.CheckingCode = m_CheckingCode;
        clientConnection.Connect(socket);
        m_Clients.Add(clientConnection);

        Debug.Log("连接上客户端！");
    }

     //关闭项目终止线程,停止服务器.
     private void OnApplicationQuit()
    {
        m_Server.Close();
        m_ListenThread.Abort();
        m_ListenThread.IsBackground = true;//关闭线程
     }

    public void SendToTargetClient<T>(ServerTCPConnection client,int command, T pack)
    {
        client.Send<T>(command, pack);
    }
}
