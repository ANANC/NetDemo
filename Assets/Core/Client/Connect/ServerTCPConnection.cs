using System.Net.Sockets;

public class ServerTCPConnection : BaseConnection
{
    private byte m_CheckingCode;
    public byte CheckingCode
    {
        set
        {
            m_CheckingCode = value;
        }
    }

    public void Connect(Socket socket)
    {
        GoogleProtoNetBody body = new ServerNetBody();

        NetPackage netPackage = new NetPackage();
        netPackage.SetNetBody(body);
        netPackage.checkingCode = m_CheckingCode;

        SetNetPackage(netPackage);

        SetSocket(socket);
    }
}
