﻿using System.Collections;
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
}
