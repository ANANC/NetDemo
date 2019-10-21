using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Msg.G2C;
using Google.Protobuf;

public class ServerNetBody : GoogleProtoNetBody
{
    protected override void RegisterParsers()
    {
        AddParser((int)CMD.Respond, Respond.Parser);
    }

    protected override void RegisterReceiver()
    {
        AddReceiveDelegate((int)CMD.Respond, RespondMessage);
    }

    //------ callback

    private void RespondMessage(IMessage message)
    {
        Respond msg = message as Respond;
        Debug.Log("服务器接收："+msg.Content);
    }
}
