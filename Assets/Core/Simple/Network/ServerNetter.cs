using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class ServerNetter : SimpleNetter
{
    public delegate void ServerMessageReceiveDelegate(uint clientId, IMessage message);
    protected class NetData : ReceiveObject
    {
        public uint clientId;
    }

    public const int CLIENT_MAX = 100;  //连接客户端的上限
    private static int s_ClientAutoId = 0; //客户端自动分配业务id

    private Dictionary<uint, ClientNetter> m_ClientDict = new Dictionary<uint, ClientNetter>();
    private List<ClientNetter> m_ClientList = new List<ClientNetter>();

    private new Dictionary<int, ServerMessageReceiveDelegate> m_ReceiveDelegateDic = new Dictionary<int, ServerMessageReceiveDelegate>();   //协议处理回调
    private Dictionary<Type, int> m_MessageToCommand = new Dictionary<Type, int>(); //协议类型对应的id

    private Thread m_ListenThread;


    public ServerNetter(int port) : base()
    {
        m_OnAcceptCallback = AcceptClient;

        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Socket.Bind(new IPEndPoint(IPAddress.Any, port));
        m_Socket.Listen(CLIENT_MAX);

        m_ListenThread = new Thread(ReceiveClient);
        m_ListenThread.Start(); //开启线程
        m_ListenThread.IsBackground = true; //后台运行
    }

    public void AddReceiveDelegate(int command, ServerMessageReceiveDelegate receiveDelegate)
    {
        m_ReceiveDelegateDic.Add(command, receiveDelegate);
    }
    public new void AddParser(int command, MessageParser parser)
    {
        m_ReceiveParserDic.Add(command, parser);
        m_MessageToCommand.Add(parser.CreateTemplate().GetType(), command);
    }


    public new void Update()
    {
        while (true)
        {
            ReceiveObject receiveObject = null;

            //从处理队列中得到需要处理的协议对象
            if (m_ReceiveQueue.TryDequeue(out receiveObject))
            {
                ServerMessageReceiveDelegate messageReceiveDelegate = null;
                if (m_ReceiveDelegateDic.TryGetValue(receiveObject.command, out messageReceiveDelegate))
                {
                    NetData netData = receiveObject as NetData;
                    //业务处理
                    messageReceiveDelegate(netData.clientId, netData.message);
                }
                continue;
            }
            break;
        }

        for(int index = 0;index< m_ClientList.Count;index++)
        {
            m_ClientList[index].Update();
        }
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
        clientNetter.Id = (uint)(s_ClientAutoId++);

        Dictionary<int, MessageParser>.Enumerator parserEnumator = m_ReceiveParserDic.GetEnumerator();
        while(parserEnumator.MoveNext())
        {
            clientNetter.AddParser(parserEnumator.Current.Key, parserEnumator.Current.Value);
        }

        Dictionary<int, ServerMessageReceiveDelegate>.Enumerator delegateEnumator = m_ReceiveDelegateDic.GetEnumerator();
        while (delegateEnumator.MoveNext())
        {
            MessageReceiveDelegate callback = (message) => { ClientEndReceive(clientNetter.Id, message); };
            clientNetter.AddReceiveDelegate(delegateEnumator.Current.Key, callback);
        }

        m_ClientDict.Add(clientNetter.Id, clientNetter);
        m_ClientList.Add(clientNetter);

        clientNetter.BeginReceive();
    }

    private void ClientEndReceive(uint id,  IMessage message)
    {
        NetData receiveObject = new NetData();
        receiveObject.clientId = id;
        receiveObject.command = m_MessageToCommand[message.GetType()]; ;
        receiveObject.message = message;
        m_ReceiveQueue.Enqueue(receiveObject);
    }

    public ClientNetter GetClient(uint id)
    {
        ClientNetter client;
        if (m_ClientDict.TryGetValue(id,out client))
        {
            return client;
        }
        return null;
    }

}
