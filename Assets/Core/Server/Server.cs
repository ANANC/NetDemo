
public class Server
{
    private Server() { }
    private static Server m_Instace;
    public static Server Instance
    {
        get
        {
            if(m_Instace == null)
            {
                m_Instace = new Server();
            }
            return m_Instace;
        }
    }

    private int m_Port;
    public int Port
    {
        set { m_Port = value; }
    }

    private byte m_CheckingCode;
    public byte CheckingCode
    {
        set
        {
            m_CheckingCode = value;
        }
    }

    private ServerTCPConnection m_Server;
    private byte[] m_ClinetDatas;


    public void Start()
    {
        m_Server = new ServerTCPConnection();
        m_Server.Connect(m_Port,m_CheckingCode);
    }

    public void Update()
    {
        m_Server.Update();
    }

    public void Destroy()
    {
        OnApplicationQuit();
    }

     //关闭项目终止线程,停止服务器.
     private void OnApplicationQuit()
    {
        m_Server.Close();
     }

}
