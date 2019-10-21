using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public abstract class BaseConnection : IConnection
{
    protected const int MIN_READ = 4;                 //包体最小长度，和服务器统一
    protected const int MAX_READ = 1024 * 1024 * 1;   //包的最大长度，和服务器统一

    protected INetPackage m_NetPackage = null;        //包体解析器
    protected NetBuffer m_NetBuffer = new NetBuffer(MAX_READ);           //网络缓存池
    protected Socket m_Client = null;              //sockect
    protected ConnectNotificationDelegate m_ConnectNotificationDelegate = null;   //连接状态事件

    public void SetSocket(Socket socket)
    {
        m_Client = socket;
        m_Client.BeginReceive(m_NetBuffer.bytes, m_NetBuffer.length, 0, SocketFlags.None, new AsyncCallback(OnRead),
            null);
    }

    public void Close()
    {
        if (m_Client != null)
        {
            try
            {
                if (m_Client.Connected)
                {
                    m_Client.Close();
                    SendNotification(ConnectNotificationType.Disconnect, "关闭连接");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
            m_Client = null;
        }
    }

    public bool IsConnected()
    {
        bool bRet = false;
        if (m_Client != null && m_Client.Connected)
        {
            bRet = !((m_Client.Poll(1000, SelectMode.SelectRead) && (m_Client.Available == 0)) || !m_Client.Connected);
        }
        return bRet;
    }

    public void Update()
    {
        if (m_NetPackage != null)
        {
            m_NetPackage.Update();
        }
    }

    // -- 写数据 -- 
    public void WriteMessage(byte[] head, byte[] body, int length)
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
            if (m_Client != null && m_Client.Connected)
            {
                byte[] payload = ms.ToArray();
                //向缓冲区写入数据
                m_Client.BeginSend(payload, 0, payload.Length, SocketFlags.None, new AsyncCallback(OnWrite), null);
            }
            else
            {
                Debug.LogWarning("WriteMessage m_Client.connected----->>false");
                SendNotification(ConnectNotificationType.Exception, "TcpClient Is Not Connected.");
            }
        }
    }

    //向链接写入数据，等待底层发送
    protected void OnWrite(IAsyncResult asr)
    {
        try
        {
            if (m_Client != null && m_Client.Connected)
            {
                {
                    m_Client.EndSend(asr);
                }
            }
            else
            {
                Debug.LogWarning("OnWrite m_Client.connected----->>false");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("OnWrite--->>>" + ex.Message);
        }
    }

    // -- 读数据 --

    public void ReceiveMessage(byte[] bytes, int length)
    {
        m_NetBuffer.length += length;

        while (m_NetBuffer.RemainingBytes() > MIN_READ)
        {
            int messageLen = m_NetBuffer.ReadInt();
            if (messageLen > MIN_READ && messageLen <= MAX_READ)
            {
                if (m_NetBuffer.RemainingBytes() >= messageLen)
                {
                    //解包
                    string msg = m_NetPackage.ReadPackage(m_NetBuffer.bytes, m_NetBuffer.index, messageLen);

                    //缓存区头指针指向该包体长度的下一位，等待重新写入数据
                    m_NetBuffer.index += messageLen;
                    if (!string.IsNullOrEmpty(msg))
                    {
                        SendNotification(ConnectNotificationType.Exception, "ReceiveMessage Error : " + msg);
                        return;
                    }
                }
                else
                {
                    //重置缓存区头指针，等待剩余数据
                    m_NetBuffer.index -= MIN_READ;
                    m_NetBuffer.MoveBytesToHead();
                    return;
                }
            }
            else
            {
                SendNotification(ConnectNotificationType.Exception, "Package Head Lenght Field Error : " + messageLen.ToString());
                Close();
            }
        }
        m_NetBuffer.MoveBytesToHead();
    }

    protected void OnRead(IAsyncResult asr)
    {
        int bytesRead = 0;
        if (m_Client != null && m_Client.Connected)
        {
            try
            {
                //读取字节流到缓冲区
                bytesRead = m_Client.EndReceive(asr);
                if (bytesRead < 4)
                {
                    //包尺寸有问题，断线处理
                    SendNotification(ConnectNotificationType.Exception, "package size error");
                    return;
                }

                ReceiveMessage(m_NetBuffer.bytes, bytesRead); //分析数据包内容，抛给逻辑层
                m_Client.BeginReceive(m_NetBuffer.bytes, m_NetBuffer.length,
                    m_NetBuffer.Capacity() - m_NetBuffer.length, SocketFlags.None, new AsyncCallback(OnRead), null);
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

    protected void SendNotification(ConnectNotificationType connectNotificationType, string message)
    {
        Debug.Log("SendNotification , " + connectNotificationType.ToString() + " , " + message);
        if (m_ConnectNotificationDelegate != null)
        {
            m_ConnectNotificationDelegate(connectNotificationType, message);
        }
    }

    public void SetConnectNotification(ConnectNotificationDelegate callback)
    {
        m_ConnectNotificationDelegate = callback;
    }

    public void SetNetPackage(INetPackage netPackage)
    {
        m_NetPackage = netPackage;
    }
}
