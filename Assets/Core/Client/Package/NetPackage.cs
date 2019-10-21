using System;
using UnityEngine;

public class NetPackage : INetPackage
{
    private INetBody m_NetBody = null;                          //包体业务处理
    private byte[] m_SendBuffer = new byte[1024 * 1024 * 1];    //缓冲区
    private byte[] m_HeadBuffer = new byte[11];                 //快速检验byte,加密算法byte,压缩byte,CRC检验int,包体编号int
    private byte[] m_HandleBuffer = null;                       //包头
    private int m_PackageIndex = 1;                             //包体编号

    private byte m_CheckingCode = 0;                            //快速校验码
    public byte checkingCode
    {
        set
        {
            m_CheckingCode = value;
        }
        get
        {
            return m_CheckingCode;
        }
    }

    public string ReadPackage(byte[] bytes, int offset, int length)
    {
        int dwStartOffset = offset;

        //快速校验码
        byte checkingCode = ClientNetUtils.ReadByteFromBytes(bytes, ref offset);
        if (!checkingCode.Equals(m_CheckingCode))
        {
            return "ReadPackage Failed , CheckingCode Is Incorrect.";
        }

        //本次包的编号
        int packageID = ClientNetUtils.ReadIntFromBytes(bytes, ref offset);

        m_HandleBuffer = new byte[length - (offset - dwStartOffset)];
        Array.Copy(bytes, offset, m_HandleBuffer, 0, m_HandleBuffer.Length);


        if (m_NetBody == null)
        {
            return "Not Set NetBody Object";
        }

        //包体解析
        string res = m_NetBody.ReadBody(m_HandleBuffer, 0, m_HandleBuffer.Length);
        if (!string.IsNullOrEmpty(res))
        {
            return res;
        }

        return string.Empty;
    }

    public bool SendPackage<T>(IConnection connection, int command, T pack)
    {
        int headSize = 0;
        int bodySize = 0;

        //包体内容
        if (!m_NetBody.SendBody(pack, command, m_SendBuffer, ref bodySize))
        {
            Debug.LogError("Send Package Body Fails .");
            return false;
        }

        m_HandleBuffer = new byte[bodySize];
        Array.Copy(m_SendBuffer, 0, m_HandleBuffer, 0, bodySize);

        //快速校验码
        ClientNetUtils.WriteByteToBytes(m_CheckingCode, m_HeadBuffer, ref headSize);
        
        //本次包的编号
        ClientNetUtils.WriteIntToBytes(m_PackageIndex++, m_HeadBuffer, ref headSize);

        connection.WriteMessage(m_HeadBuffer, m_HandleBuffer, m_HandleBuffer.Length);
        return true;
    }

    public void Update()
    {
        if (m_NetBody != null)
        {
            m_NetBody.Excute();
        }
    }


    public void SetNetBody(INetBody netBody)
    {
        m_NetBody = netBody;
    }
}
