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
        m_Netter.m_ClientRegisterCallback = InitClientRegister;
    }

    private void InitClientRegister(ClientNetter client)
    {
        client.AddParser((int)Msg.C2G.CMD.AuthReq, Msg.C2G.AuthReq.Parser);
        client.AddParser((int)Msg.C2G.CMD.CmessageReq, Msg.C2G.CMESSAGEReq.Parser);

        client.AddReceiveDelegate((int)Msg.C2G.CMD.AuthReq, (message) => { AuthMessage(client, message); });
        client.AddReceiveDelegate((int)Msg.C2G.CMD.CmessageReq, (message) => { RespondMessage(client, message); });
    }

    // -- 业务处理 --

    // -- Client to Server --

    private void AuthMessage(ClientNetter client , IMessage message)
    {
        Msg.C2G.AuthReq authReq = message as Msg.C2G.AuthReq;

        NetTestMgr.ShowStrContentEvent(true, "认证客户端");

        SendAuthRsp(client);
    }

    private void RespondMessage(ClientNetter client, IMessage message)
    {
        Msg.C2G.CMESSAGEReq msg = message as Msg.C2G.CMESSAGEReq;
        NetTestMgr.ShowStrContentEvent(true, string.Format("收到客户端{0}：{1}", client.Id, msg.ClientMessage));

        SendMessageRsp(client,msg.ClientMessage);
    }

    // -- Server to Client --
    public void SendAuthRsp(ClientNetter client)
    {
        Msg.G2C.AuthRsp msg = new Msg.G2C.AuthRsp();
        msg.UserId = client.Id.ToString();
        client.Send<Msg.G2C.AuthRsp>(((int)Msg.G2C.CMD.AuthRsp), msg);
    }

    public void SendMessageRsp(ClientNetter client, string message)
    {
        NetTestMgr.ShowStrContentEvent(true, string.Format("发送：{0}", message));
        Msg.G2C.SMESSAGERsp respond = new Msg.G2C.SMESSAGERsp();
        respond.ClientMessage = message;
        client.Send<Msg.G2C.SMESSAGERsp>(((int)Msg.G2C.CMD.SmessageRsp), respond);
    }
}
