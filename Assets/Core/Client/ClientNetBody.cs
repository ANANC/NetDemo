using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Msg.C2G;
using Google.Protobuf;

public class ClientNetBody : GoogleProtoNetBody
{
    protected override void RegisterParsers()
    {
        AddParser((int)CMD.Message, MESSAGE.Parser);
    }

    protected override void RegisterReceiver()
    {
        AddReceiveDelegate((int)CMD.Message, ReceiverMessage);
    }

    //------ callback

    private void ReceiverMessage(IMessage message)
    {
        MESSAGE msg = message as MESSAGE;
        Debug.Log("客户端接收："+msg.Content);
    }
}
