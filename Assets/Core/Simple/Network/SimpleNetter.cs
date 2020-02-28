using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class SimpleNetter
{
    //*  数据  *//    
    public delegate void MessageReceiveDelegate(IMessage message);
    public enum ConnectNotificationType
    {
        Exception,
        Disconnect,
    }

    // 协议结构
    protected class ReceiveObject
    {
        public int command;
        public IMessage message;
        public void Reset()
        {
            command = 0;
            message = null;
        }
    }

    public const int MIN_READ = 4;                 //包体最小长度
    public const int MAX_READ = 1024 * 1024 * 1;   //包的最大长度

    // -- 连接 --
    protected Socket m_Socket;

    // -- 缓存池 --
    private byte[] m_ByteBuffer = new byte[MAX_READ];
    private int m_ByteFirstIndex = 0;
    private int m_ByteEndIndex = 0;

    // -- 协议内容 -- 
    private byte[] m_SendBuffer = new byte[MAX_READ];           //缓冲区
    private byte[] m_HeadBuffer = new byte[13];                  //包头
    private byte[] m_HandleBuffer = null;                       //包体
    private int m_PackageIndex = 1;                             //包体编号

    public byte m_CheckingCode = 123;                           //快速校验码
    public byte checkingCode
    {
        get { return m_CheckingCode; }
        set { m_CheckingCode = value; }
    }

    // -- 协议 --

    protected ConcurrentQueue<ReceiveObject> m_ReceiveQueue = new ConcurrentQueue<ReceiveObject>();                           //处理队列
    protected Dictionary<int, MessageParser> m_ReceiveParserDic = new Dictionary<int, MessageParser>();                       //协议解析器
    protected Dictionary<int, MessageReceiveDelegate> m_ReceiveDelegateDic = new Dictionary<int, MessageReceiveDelegate>();   //协议处理回调


    // -- 状态回调 --
    protected Action<IAsyncResult> m_OnConnectCallback;
    protected Action<IAsyncResult> m_OnAcceptCallback;


    //*  实现  *//

    // -- 初始化 --
    public void AddParser(int command, MessageParser parser)
    {
        m_ReceiveParserDic.Add(command, parser);
    }

    public void AddReceiveDelegate(int command, MessageReceiveDelegate receiveDelegate)
    {
        m_ReceiveDelegateDic.Add(command, receiveDelegate);
    }

    public void BeginConnect(string host, int port)
    {
        m_Socket.BeginConnect(host, port, OnConnect, null);
    }

    private void OnConnect(IAsyncResult asr)
    {
        if (m_OnConnectCallback != null)
        {
            m_OnConnectCallback(asr);
        }
    }

    public void BeginAccept()
    {
        m_Socket.BeginAccept(OnAccept, null);   
    }

    private void OnAccept(IAsyncResult ar)
    {
        if (m_OnAcceptCallback != null)
        {
            m_OnAcceptCallback(ar);
        }
    }

    public void BeginReceive()
    {
       m_Socket.BeginReceive(m_ByteBuffer, 0, m_SendBuffer.Length, SocketFlags.None, OnRead, null);
    }

    // -- 对外接口 --

    public Socket GetSocket()
    {
        return m_Socket;
    }

    public void Update()
    {
        Excute();
    }

    //处理接收到的协议
    private void Excute()
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

    /// <summary>
    /// 发送信息
    /// </summary>
    public void Send<T>(int command, T pack)
    {
        if (!SendPackage<T>(command, pack))
        {
            SendNotification(ConnectNotificationType.Exception, "Send Package Fails!");
        }
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    public void Close()
    {
        if (m_Socket != null)
        {
            try
            {
                if (m_Socket.Connected)
                {
                    m_Socket.Close();
                    SendNotification(ConnectNotificationType.Disconnect, "关闭连接");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
            m_Socket = null;
        }
    }

    /// <summary>
    /// 是否连接
    /// </summary>
    /// <returns></returns>
    public bool IsConnected()
    {
        bool bRet = false;
        if (m_Socket != null && m_Socket.Connected)
        {
            bRet = !((m_Socket.Poll(1000, SelectMode.SelectRead) && (m_Socket.Available == 0)) || !m_Socket.Connected);
        }
        return bRet;
    }

    // -- 写数据 -- 

    // 业务发包
    private bool SendPackage<T>(int command, T pack)
    {
        int headSize = 0;
        int bodySize = 0;

        //包体填充
        if (!SendBody(pack, command, m_SendBuffer, ref bodySize))
        {
            SendNotification(ConnectNotificationType.Exception, "Send Package Body Fails .");
            return false;
        }

        m_HandleBuffer = new byte[bodySize];
        Array.Copy(m_SendBuffer, 0, m_HandleBuffer, 0, bodySize);

        // 包头填充

        // 2.快速校验码
        //WriteByteToBytes(m_CheckingCode, m_HeadBuffer, ref headSize);

        //// 1.本次包的编号
        //WriteIntToBytes(m_PackageIndex++, m_HeadBuffer, ref headSize);

        //cola测试
        WriteByteToBytes((byte)'S', m_HeadBuffer, ref headSize);
        WriteByteToBytes((byte)'G', m_HeadBuffer, ref headSize);
        WriteByteToBytes((byte)'E', m_HeadBuffer, ref headSize);
        WriteByteToBytes(0, m_HeadBuffer, ref headSize);//Encryption
        WriteByteToBytes(0, m_HeadBuffer, ref headSize);//Compress
        WriteIntToBytes(0, m_HeadBuffer, ref headSize);//CRC
        WriteIntToBytes(1, m_HeadBuffer, ref headSize);//PackageIndex

        //headSize = 13;

        WriteMessage(m_HeadBuffer, m_HandleBuffer, m_HandleBuffer.Length);
        return true;
    }

    // 业务填充
    public bool SendBody<T>(T pack, int command, byte[] bytes, ref int length)
    {
        //CodedOutputStream output = new CodedOutputStream(bytes);

        ////协议代号
        //output.WriteInt32(command);

        ////写入包内容
        //IMessage message = pack as IMessage;
        //message.WriteTo(output);

        ////包长度
        //length = (int)output.Position;

        //output.Dispose();

        int index = 0;
        WriteIntToBytes(command, bytes, ref index);
        IMessage message = pack as IMessage;
        byte[] messageBytes = message.ToByteArray();
        for(int i = 0;i < messageBytes.Length;i++)
        {
            WriteByteToBytes(messageBytes[i], bytes, ref index);
        }
        length = index;
        return true;
    }


    // 数据转为字节，写入缓存区
    private void WriteMessage(byte[] head, byte[] body, int length)
    {
        MemoryStream ms = null;
        using (ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter writer = new BinaryWriter(ms);
            int msglen = length + head.Length;

            writer.Write(msglen);
            writer.Write(head);
            writer.Write(body, 0, length);
            writer.Flush();
            if (m_Socket != null && m_Socket.Connected)
            {
                byte[] payload = ms.ToArray();
                //向缓冲区写入数据
                m_Socket.BeginSend(payload, 0, payload.Length, SocketFlags.None, new AsyncCallback(OnWrite), null);
            }
            else
            {
                SendNotification(ConnectNotificationType.Exception, "TcpClient Is Not Connected.");
            }
        }
    }

    //向Socket写入数据，等待底层发送
    private void OnWrite(IAsyncResult asr)
    {
        try
        {
            if (m_Socket != null && m_Socket.Connected)
            {
                {
                    m_Socket.EndSend(asr);
                }
            }
            else
            {
                SendNotification(ConnectNotificationType.Disconnect, "OnWrite m_Client.connected----->>false");
            }
        }
        catch (Exception ex)
        {
            SendNotification(ConnectNotificationType.Exception, "OnWrite--->>>" + ex.Message);
        }
    }


    // -- 读数据 --

    // 监听链接，读取字节流
    private void OnRead(IAsyncResult asr)
    {
        if (m_Socket != null && m_Socket.Connected)
        {
            try
            {
                //读取字节流到缓冲区
                int bytesRead = m_Socket.EndReceive(asr);
                if (bytesRead > 0)
                {
                    ReceiveMessage(bytesRead); //分析数据包内容，抛给逻辑层
                    m_Socket.BeginReceive(m_ByteBuffer, m_ByteEndIndex, m_ByteBuffer.Length - m_ByteEndIndex, SocketFlags.None, OnRead, null);
                }
            }
            catch (Exception ex)
            {
                SendNotification(ConnectNotificationType.Exception, "OnRead Error : " + ex.Message);
            }
        }
        else
        {
            SendNotification(ConnectNotificationType.Disconnect, "关闭连接");
        }
    }

    // 接收字节，截取包
    private void ReceiveMessage(int length)
    {
        m_ByteEndIndex += length;

        int mainingBytesLength = m_ByteEndIndex - m_ByteFirstIndex;

        while (mainingBytesLength > MIN_READ)
        {
            int messageLen = ReadIntFromBytes(m_ByteBuffer, m_ByteFirstIndex);

            if (messageLen > MIN_READ && messageLen <= MAX_READ)
            {
                if (mainingBytesLength < messageLen)
                {
                    //粘包等待下一个包数据
                    return;
                }

                // 读取了包长度
                m_ByteFirstIndex += 4;

                //解包
                string msg = ReadPackage(m_ByteBuffer, m_ByteFirstIndex, messageLen);

                //缓存区头指针指向该包体长度的下一位，等待重新写入数据
                m_ByteFirstIndex += messageLen;

                if (!string.IsNullOrEmpty(msg))
                {
                    SendNotification(ConnectNotificationType.Exception, "ReceiveMessage Error : " + msg);
                }
            }
            else
            {
                SendNotification(ConnectNotificationType.Exception, "Package Head Lenght Field Error : " + messageLen.ToString());
                Close();
                break;
            }

            mainingBytesLength = m_ByteEndIndex - m_ByteFirstIndex;
        }

        if (m_ByteFirstIndex != 0)
        {
            if (mainingBytesLength > 0)
            {
                Array.Copy(m_ByteBuffer, m_ByteFirstIndex, m_ByteBuffer, 0, mainingBytesLength);
            }

            m_ByteFirstIndex = 0;
            m_ByteEndIndex = mainingBytesLength;
        }
    }

    // 解包，包头解析
    private string ReadPackage(byte[] bytes, int offset, int length)
    {
        int dwStartOffset = offset;

        // 包头解析

        // 1.快速校验码
        //byte checkingCode = ReadByteFromBytes(bytes, ref offset);
        //if (!checkingCode.Equals(m_CheckingCode))
        //{
        //    return "ReadPackage Failed , CheckingCode Is Incorrect.";
        //}

        // 2.本次包的编号
        //int packageID = ReadIntFromBytes(bytes, ref offset);

        // cola - 测试
        offset += 13;

        m_HandleBuffer = new byte[length - (offset - dwStartOffset)];
        Array.Copy(bytes, offset, m_HandleBuffer, 0, m_HandleBuffer.Length);

        //包体解析
        string res = ReadBody(m_HandleBuffer, 0, m_HandleBuffer.Length);
        if (!string.IsNullOrEmpty(res))
        {
            return res;
        }

        return string.Empty;
    }

    // 解析包体
    private string ReadBody(byte[] bytes, int offset, int length)
    {
       // CodedInputStream input = new CodedInputStream(bytes, offset, length);

        int command = ReadIntFromBytes(bytes, ref offset);

       // int command = input.ReadInt32();
        MessageParser messageParser = null;

        //得到协议的解释器
        if (!m_ReceiveParserDic.TryGetValue(command, out messageParser))
        {
            return "Do Not Register Parser Function For : " + command;
        }

        //字节转为数据结构
        IMessage message = messageParser.ParseFrom(bytes, offset, (int)(length - offset));

        //添加到处理队列中
        ReceiveObject receiveObject = new ReceiveObject();
        receiveObject.command = command;
        receiveObject.message = message;
        m_ReceiveQueue.Enqueue(receiveObject);

       // input.Dispose();
        return string.Empty;
    }

    // -- Util --

    public static void WriteByteToBytes(byte value, byte[] bytes, ref int offset)
    {
        bytes[offset] = value;
        offset++;
    }

    public static byte ReadByteFromBytes(byte[] bytes, ref int offset)
    {
        byte value = bytes[offset];
        offset++;
        return value;
    }

    public static void WriteIntToBytes(int value, byte[] bytes, ref int offset)
    {
        bytes[offset + 3] = (byte)((value >> 24));
        bytes[offset + 2] = (byte)((value >> 16));
        bytes[offset + 1] = (byte)((value >> 8));
        bytes[offset + 0] = (byte)((value));
        offset += 4;
    }

    public static int ReadIntFromBytes(byte[] bytes, ref int offset)
    {
        int value = ReadIntFromBytes(bytes, offset);
        offset += 4;
        return value;
    }

    public static int ReadIntFromBytes(byte[] bytes, int offset)
    {
        int value;
        value = (int)((bytes[offset + 0])
                   | ((bytes[offset + 1]) << 8)
                   | ((bytes[offset + 2]) << 16)
                   | ((bytes[offset + 3]) << 24));
        return value;
    }

    // -- 输出 --
    public void SendNotification(ConnectNotificationType connectNotificationType, string message)
    {
        Debug.Log("【Network】SendNotification Type:" + connectNotificationType.ToString() + " Msg: " + message);
    }

}
