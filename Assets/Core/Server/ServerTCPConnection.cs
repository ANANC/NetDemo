using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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

        NetTestMgr.ShowStrContentEvent(true, "连接客户端");
    }

    private void NetBodyRegisterReceiver(ServerClientTCPConnection client)
    {
        client.AddReceiveDelegate((int)Msg.C2G.CMD.AuthReq, (message) => { AuthMessage(client, message); });
        client.AddReceiveDelegate((int)Msg.C2G.CMD.CmessageReq, (message) => { RespondMessage(client, message); });
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

    private void AuthMessage(ServerClientTCPConnection client, IMessage message)
    {
        NetTestMgr.ShowStrContentEvent(true, "认证客户端");

        Msg.G2C.AuthRsp msg = new Msg.G2C.AuthRsp();
        m_Clients.Add(client);
        msg.UserId = m_Clients.Count.ToString();
        client.m_Id = msg.UserId;
        client.Send<Msg.G2C.AuthRsp>(((int)Msg.G2C.CMD.AuthRsp), msg);
    }

    private void RespondMessage(ServerClientTCPConnection client, IMessage message)
    {
        Msg.C2G.CMESSAGEReq msg = message as Msg.C2G.CMESSAGEReq;
        NetTestMgr.ShowStrContentEvent(true, string.Format("收到客户端{0}：{1}", client.m_Id, msg.ClientMessage));

        NetTestMgr.ShowStrContentEvent(true, string.Format("发送：{0}", msg.ClientMessage));
        Msg.G2C.SMESSAGERsp respond = new Msg.G2C.SMESSAGERsp();
        respond.ClientMessage = msg.ClientMessage;
        client.Send<Msg.G2C.SMESSAGERsp>(((int)Msg.G2C.CMD.SmessageRsp), respond);
    }
}
