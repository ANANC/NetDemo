using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerNetUser 
{
    private ServerNetter m_Netter;

    public ServerNetUser(int port)
    {
        m_Netter = new ServerNetter(port);
    }

    private void InitClientRegister()
    {
        m_Netter.AddParser((int)Msg.C2G.CMD.AuthReq, Msg.C2G.AuthReq.Parser);
        m_Netter.AddParser((int)Msg.C2G.CMD.CmessageReq, Msg.C2G.CMESSAGEReq.Parser);

        m_Netter.AddReceiveDelegate((int)Msg.C2G.CMD.AuthReq, AuthMessage);
        m_Netter.AddReceiveDelegate((int)Msg.C2G.CMD.CmessageReq, RespondMessage);
    }

    public void Update()
    {
        m_Netter.Update();
    }

    // -- 业务处理 --

    // -- Client to Server --

    private void AuthMessage(uint clientId , IMessage message)
    {
        Msg.C2G.AuthReq authReq = message as Msg.C2G.AuthReq;

        NetTestMgr.ShowStrContentEvent(true, "认证客户端");

        SendAuthRsp(clientId);
    }

    private void RespondMessage(uint clientId, IMessage message)
    {
        Msg.C2G.CMESSAGEReq msg = message as Msg.C2G.CMESSAGEReq;
        NetTestMgr.ShowStrContentEvent(true, string.Format("收到客户端{0}：{1}", clientId, msg.ClientMessage));

        SendMessageRsp(clientId, msg.ClientMessage);
    }

    // -- Server to Client --
    public void SendAuthRsp(uint clientId)
    {
        Msg.G2C.AuthRsp msg = new Msg.G2C.AuthRsp();
        msg.UserId = clientId;
        ClientNetter client = m_Netter.GetClient(clientId);
        client.Send<Msg.G2C.AuthRsp>(((int)Msg.G2C.CMD.AuthRsp), msg);
    }

    public void SendMessageRsp(uint clientId, string message)
    {
        NetTestMgr.ShowStrContentEvent(true, string.Format("发送：{0}", message));
        Msg.G2C.SMESSAGERsp respond = new Msg.G2C.SMESSAGERsp();
        respond.ClientMessage = message;
        ClientNetter client = m_Netter.GetClient(clientId);
        client.Send<Msg.G2C.SMESSAGERsp>(((int)Msg.G2C.CMD.SmessageRsp), respond);
    }
}
