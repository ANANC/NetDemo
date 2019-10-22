using Google.Protobuf;
using Msg.G2C;
using System.Net.Sockets;
using UnityEngine;

public class ServerTCPConnection : BaseConnection
{
    public void Connect(Socket socket, byte checkingCode)
    {
        ServerNetBody body = new ServerNetBody();
        NetBodyRegisterReceiver(body);

        NetPackage netPackage = new NetPackage();
        netPackage.SetNetBody(body);
        netPackage.checkingCode = checkingCode;

        SetNetPackage(netPackage);

        SetSocket(socket);

    }

    private void NetBodyRegisterReceiver(ServerNetBody body)
    {
        body.AddReceiveDelegate((int)CMD.Respond, RespondMessage);
    }

    //------ callback

    private void RespondMessage(IMessage message)
    {
        Respond msg = message as Respond;
        Server.Instance.RespondMessage(this, msg);
    }
}
