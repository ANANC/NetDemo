﻿using Google.Protobuf;
using System;

public class ClientNetUser 
{
    private ClientNetter m_Netter;
    public Action<string> m_ContentShow;

    public ClientNetUser()
    {
        m_Netter = new ClientNetter();
        InitRegister();
    }

    private void InitRegister()
    {
        // 解释器
        m_Netter.AddParser((int)Msg.G2C.CMD.AuthRsp, Msg.G2C.AuthRsp.Parser);
        m_Netter.AddParser((int)Msg.G2C.CMD.SmessageRsp, Msg.G2C.SMESSAGERsp.Parser);

        // 业务处理
        m_Netter.AddReceiveDelegate((int)Msg.G2C.CMD.AuthRsp, ReceiverAuth);
        m_Netter.AddReceiveDelegate((int)Msg.G2C.CMD.SmessageRsp, ReceiverMessage);
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void Connect(string host, int port)
    {
        m_Netter.BeginConnect(host, port);
    }

    public void Update()
    {
        m_Netter.Update();
    }

    public void Close()
    {
        m_Netter.Close();
    }

    // -- 业务处理 --

    // -- Client to Server --

    /// <summary>
    /// 发送登录认证
    /// </summary>
    public void SendAuthReq()
    {
        Show("客户端请求认证");

        Msg.C2G.AuthReq pack = new Msg.C2G.AuthReq();
        m_Netter.Send<Msg.C2G.AuthReq>((int)Msg.C2G.CMD.AuthReq, pack);
    }

    /// <summary>
    /// 发送信息
    /// </summary>
    public void SendMessage(string message)
    {
        Show("发送：" + message);

        Msg.C2G.CMESSAGEReq pack = new Msg.C2G.CMESSAGEReq();
        pack.ClientMessage = message;
        m_Netter.Send<Msg.C2G.CMESSAGEReq>((int)Msg.C2G.CMD.CmessageReq, pack);
    }


    // -- Server to Client --

    /// <summary>
    /// 接收登录认证
    /// </summary>
    private void ReceiverAuth(IMessage message)
    {
        Msg.G2C.AuthRsp msg = message as Msg.G2C.AuthRsp;
        m_Netter.Id = msg.UserId;
        Show(string.Format("接收：客户端{0}认证通过", msg.UserId));
    }

    /// <summary>
    /// 接收信息
    /// </summary>
    private void ReceiverMessage(IMessage message)
    {
        Msg.G2C.SMESSAGERsp msg = message as Msg.G2C.SMESSAGERsp;
        Show(string.Format("接收：{0}", msg.ClientMessage));
    }

    private void Show(string content)
    {
        if(m_ContentShow !=null)
        {
            m_ContentShow(content);
        }
    }
}
