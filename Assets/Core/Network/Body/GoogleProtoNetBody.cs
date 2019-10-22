using System.Collections.Concurrent;
using System.Collections.Generic;
using Google.Protobuf;

public delegate void MessageReceiveDelegate(IMessage message);

public class GoogleProtoNetBody : INetBody
{
    private class ReceiveObject 
    {
        public int command;
        public IMessage message;
        public void Reset()
        {
            command = 0;
            message = null;
        }
    }

    private ConcurrentQueue<ReceiveObject> m_ReceiveQueue = new ConcurrentQueue<ReceiveObject>();                           //处理队列
    private Dictionary<int, MessageParser> m_ReceiveParserDic = new Dictionary<int, MessageParser>();                       //协议解析器
    private Dictionary<int, MessageReceiveDelegate> m_ReceiveDelegateDic = new Dictionary<int, MessageReceiveDelegate>();   //协议处理回调

    public GoogleProtoNetBody()
    {
        RegisterParsers();
        RegisterReceiver();
    }

    public void Excute()
    {
        while (true)
        {
            ReceiveObject receiveObject = null;

            //从处理队列中得到需要处理的协议对象
            if (m_ReceiveQueue.TryDequeue(out receiveObject))
            {
                MessageReceiveDelegate messageReceiveDelegate = null;
                if (m_ReceiveDelegateDic.TryGetValue(receiveObject.command, out messageReceiveDelegate))
                {
                    //业务处理
                    messageReceiveDelegate(receiveObject.message);
                }
                continue;
            }
            break;
        }
    }

    public string ReadBody(byte[] bytes, int offset, int length)
    {
        CodedInputStream input = new CodedInputStream(bytes, offset, length);
        int command = input.ReadInt32();
        MessageParser messageParser = null;

        //得到协议的解释器
        if (!m_ReceiveParserDic.TryGetValue(command, out messageParser))
        {
            return "Do Not Register Parser Function For : " + command;
        }

        //字节转为数据结构
        IMessage message = messageParser.ParseFrom(bytes, (int)input.Position, (int)(length - input.Position));

        //添加到处理队列中
        ReceiveObject receiveObject = new ReceiveObject();
        receiveObject.command = command;
        receiveObject.message = message;
        m_ReceiveQueue.Enqueue(receiveObject);

        input.Dispose();
        return string.Empty;
    }

    public bool SendBody<T>(T pack, int command, byte[] bytes, ref int length)
    {
        CodedOutputStream output = new CodedOutputStream(bytes);

        //协议代号
        output.WriteInt32(command);

        //写入包内容
        IMessage message = pack as IMessage;
        message.WriteTo(output);

        //包长度
        length = (int)output.Position;

        output.Dispose();
        return true;
    }

    protected virtual void RegisterParsers() { }
    protected virtual void RegisterReceiver() { }

    protected void AddParser(int command, MessageParser parser)
    {
        m_ReceiveParserDic.Add(command, parser);
    }

    public void AddReceiveDelegate(int command, MessageReceiveDelegate receiveDelegate)
    {
        m_ReceiveDelegateDic.Add(command, receiveDelegate);
    }
}
