public class Client 
{
    private byte m_CheckingCode;
    public byte CheckingCode
    {
        set
        {
            m_CheckingCode = value;
        }
    }

    private ClientTCPConnection m_Connection = null;

    public void Start()
    {
        m_Connection = new ClientTCPConnection();
    }

    public void Content(string host,int port)
    {
        m_Connection.Connect(host, port);
    }

    public void Close()
    {
        m_Connection.Close();
    }

    public void Update()
    {
        m_Connection.Update();
    }

    public void Destroy()
    {
        Close();
    }

    public void Send<T>(int command, T pack)
    {
        m_Connection.Send<T>(command, pack);
    }
}
