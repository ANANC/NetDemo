using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ServerTCPConnection : BaseConnection
{
    private int m_Port;
    private byte m_CheckingCode;

    private List<ServerClientTCPConnection> m_Clients = new List<ServerClientTCPConnection>();
    private Thread m_ListenThread;

    public void Connect(int port,byte checkingCode)
    {
        m_Port = port;
        m_CheckingCode = checkingCode;

        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Socket.Bind(new IPEndPoint(IPAddress.Any, m_Port));
        m_Socket.Listen(100);

        m_ListenThread = new Thread(ReceiveClient);
        m_ListenThread.Start(); //开启线程
        m_ListenThread.IsBackground = true; //后台运行
    }
    private void ReceiveClient()
    {
        while (true)
        {
            m_Socket.BeginAccept(AcceptClient, null); //开始接受客户端连接请求            
            Thread.Sleep(1000);//每隔1s检测 有没有连接我            
        }
    }

    private void AcceptClient(IAsyncResult ar)
    {
        Socket socket = m_Socket.EndAccept(ar);
        ServerClientTCPConnection clientConnection = new ServerClientTCPConnection();
        clientConnection.Connect(socket, m_CheckingCode);
        NetBodyRegisterReceiver(clientConnection);
        m_Clients.Add(clientConnection);

        Debug.Log("连接上客户端！");
    }

    private void NetBodyRegisterReceiver(ServerClientTCPConnection client)
    {
        client.AddReceiveDelegate((int)Msg.C2G.CMD.Message, (message) => { RespondMessage(client, message); });
    }

    public new void Update()
    {
        base.Update();
        for (int index = 0; index < m_Clients.Count; index++)
        {
            m_Clients[index].Update();
        }
    }

    public new void Close()
    {
        base.Close();
        m_ListenThread.Abort();
        m_ListenThread.IsBackground = true;//关闭线程
    }

    //------ callback

    private void RespondMessage(ServerClientTCPConnection client, IMessage message)
    {
        Msg.C2G.MESSAGE msg = message as Msg.C2G.MESSAGE;
        Debug.Log("服务器收到客户端（"+client.Id()+"）信息：" + msg.Content);

        Msg.G2C.Respond respond = new Msg.G2C.Respond();
        respond.Content = "服务器回复：" + msg.Content;
        client.Send<Msg.G2C.Respond>(((int)Msg.G2C.CMD.Respond), respond);
    }
}
