using Google.Protobuf;
using System;

public class ServerNetUser 
{
    private ServerNetter m_Netter;
    public Action<string> m_ContentShow;


    public ServerNetUser(int port)
    {
        m_Netter = new ServerNetter(port);
        InitClientRegister();
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

    public void Close()
    {
        m_Netter.Close();
    }

    // -- 业务处理 --

    // -- Client to Server --

    private void AuthMessage(uint clientId , IMessage message)
    {
        Msg.C2G.AuthReq authReq = message as Msg.C2G.AuthReq;

        Show("客户端请求认证:"+ clientId);

        SendAuthRsp(clientId);
    }

    private void RespondMessage(uint clientId, IMessage message)
    {
        Msg.C2G.CMESSAGEReq msg = message as Msg.C2G.CMESSAGEReq;
        Show(string.Format("From {0}：{1}", clientId, msg.ClientMessage));

        SendMessageRsp(clientId, msg.ClientMessage);
    }

    // -- Server to Client --
    public void SendAuthRsp(uint clientId)
    {
        Show("客户端认证通过:" + clientId);

        Msg.G2C.AuthRsp msg = new Msg.G2C.AuthRsp();
        msg.UserId = clientId;
        ClientNetter client = m_Netter.GetClient(clientId);
        client.Send<Msg.G2C.AuthRsp>(((int)Msg.G2C.CMD.AuthRsp), msg);
    }

    public void SendMessageRsp(uint clientId, string message)
    {
        Show(string.Format("SendTo {1}：{0}", message,clientId));
        Msg.G2C.SMESSAGERsp respond = new Msg.G2C.SMESSAGERsp();
        respond.ClientMessage = message;
        ClientNetter client = m_Netter.GetClient(clientId);
        client.Send<Msg.G2C.SMESSAGERsp>(((int)Msg.G2C.CMD.SmessageRsp), respond);
    }

    private void Show(string content)
    {
        if (m_ContentShow != null)
        {
            m_ContentShow(content);
        }
    }

}
