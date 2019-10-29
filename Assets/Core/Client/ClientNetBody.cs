using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;

public class ClientNetBody : GoogleProtoNetBody
{
    protected override void RegisterParsers()
    {
        AddParser((int)Msg.G2C.CMD.AuthRsp, Msg.G2C.AuthRsp.Parser);
        AddParser((int)Msg.G2C.CMD.SmessageRsp, Msg.G2C.SMESSAGERsp.Parser);
    }

    protected override void RegisterReceiver()
    {
        AddReceiveDelegate((int)Msg.G2C.CMD.AuthRsp, ReceiverAuth);
        AddReceiveDelegate((int)Msg.G2C.CMD.SmessageRsp, ReceiverMessage);
    }

    //------ callback

    private void ReceiverAuth(IMessage message)
    {
        Msg.G2C.AuthRsp msg = message as Msg.G2C.AuthRsp;
        NetTestMgr.ShowStrContentEvent(false, string.Format("接收：客户端{0}认证通过", msg.UserId));
    }

    private void ReceiverMessage(IMessage message)
    {
        Msg.G2C.SMESSAGERsp msg = message as Msg.G2C.SMESSAGERsp;
        NetTestMgr.ShowStrContentEvent(false, string.Format("接收：{0}", msg.ClientMessage));
    }
}
