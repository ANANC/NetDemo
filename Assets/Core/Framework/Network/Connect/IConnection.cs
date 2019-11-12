
using System.Net.Sockets;

public delegate void ConnectNotificationDelegate(ConnectNotificationType connectNotificationType, string message);

public interface IConnection
{
    void SetSocket(Socket socket);
    void Close();
    bool IsConnected();
    void WriteMessage(byte[] head, byte[] body, int length);
    void ReceiveMessage(byte[] bytes, int length);
    void SetConnectNotification(ConnectNotificationDelegate callback);
    void SetNetPackage(INetPackage netPackage);
}
