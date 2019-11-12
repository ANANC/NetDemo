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

    // 协议结构
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

    public const int MAX_READ = 1024 * 1024 * 1;   //包的最大长度

    // -- 连接 --
    public Socket m_Socket;
    public NetBuffer m_NetBuffer = new NetBuffer(MAX_READ);           //网络缓存池

    // -- 协议内容 -- 
    private byte[] m_SendBuffer = new byte[MAX_READ];           //缓冲区
    private byte[] m_HeadBuffer = new byte[5];                  //包头
    private byte[] m_HandleBuffer = null;                       //包体
    private int m_PackageIndex = 1;                             //包体编号

    public byte m_CheckingCode = 123;                           //快速校验码

    // -- 协议 --

    private ConcurrentQueue<ReceiveObject> m_ReceiveQueue = new ConcurrentQueue<ReceiveObject>();                           //处理队列
    private Dictionary<int, MessageParser> m_ReceiveParserDic = new Dictionary<int, MessageParser>();                       //协议解析器
    private Dictionary<int, MessageReceiveDelegate> m_ReceiveDelegateDic = new Dictionary<int, MessageReceiveDelegate>();   //协议处理回调


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

    // -- 对外接口 --

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

    // -- 写数据 -- 

    // 业务发包
    private bool SendPackage<T>(int command, T pack)
    {
        int headSize = 0;
        int bodySize = 0;

        //包体内容
        if (!SendBody(pack, command, m_SendBuffer, ref bodySize))
        {
            SendNotification(ConnectNotificationType.Exception, "Send Package Body Fails .");
            return false;
        }

        m_HandleBuffer = new byte[bodySize];
        Array.Copy(m_SendBuffer, 0, m_HandleBuffer, 0, bodySize);

        //快速校验码
        NetUtils.WriteByteToBytes(m_CheckingCode, m_HeadBuffer, ref headSize);

        //本次包的编号
        NetUtils.WriteIntToBytes(m_PackageIndex++, m_HeadBuffer, ref headSize);

        WriteMessage(m_HeadBuffer, m_HandleBuffer, m_HandleBuffer.Length);
        return true;
    }

    // 业务填充
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




    // -- 输出 --
    public void SendNotification(ConnectNotificationType connectNotificationType, string message)
    {
        Debug.Log("【Network】SendNotification Type:" + connectNotificationType.ToString() + " Msg: " + message);
    }

}
