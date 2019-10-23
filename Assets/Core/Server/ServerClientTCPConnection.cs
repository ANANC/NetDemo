﻿using Google.Protobuf;
using Msg.C2G;
using System.Net.Sockets;
using UnityEngine;

public class ServerClientTCPConnection : BaseConnection
{
    ServerNetBody m_NetBody;
    public void Connect(Socket socket, byte checkingCode)
    {
        m_NetBody = new ServerNetBody();
        RegisterParsers();

        NetPackage netPackage = new NetPackage();
        netPackage.SetNetBody(m_NetBody);
        netPackage.checkingCode = checkingCode;

        SetNetPackage(netPackage);

        SetSocket(socket);
    }

    protected void RegisterParsers()
    {
        m_NetBody.AddParser((int)CMD.Message, MESSAGE.Parser);
    }

    public void AddReceiveDelegate(int command, MessageReceiveDelegate receiveDelegate)
    {
        m_NetBody.AddReceiveDelegate(command, receiveDelegate);
    }

    public int Id()
    {
        return m_Socket.AddressFamily.GetHashCode();
    }
}