using System;
using UnityEngine;

public class NetBuffer
{
    /// <summary>
    /// 用于存储接收到的服务器数据
    /// </summary>
    private byte[] m_ByteBuffer = null;
    private int m_ByteBufferIndex = 0;
    private int m_ByteBufferLength = 0;
    private int m_ByteBufferCapacity = 0;

    public NetBuffer(int capacity)
    {
        m_ByteBufferCapacity = capacity;
        m_ByteBuffer = new byte[m_ByteBufferCapacity];
        Reset();
    }

    public int Capacity()
    {
        return m_ByteBufferCapacity;
    }

    public int RemainingBytes()
    {
        return m_ByteBufferLength - m_ByteBufferIndex;
    }


    public byte[] bytes
    {
        get
        {
            return m_ByteBuffer;
        }
    }

    public int index
    {
        get
        {
            return m_ByteBufferIndex;
        }
        set
        {
            m_ByteBufferIndex = value;
        }
    }

    public int length
    {
        get
        {
            return m_ByteBufferLength;
        }
        set
        {
            m_ByteBufferLength = value;
        }
    }

    public void MoveBytesToHead()
    {
        int newLength = m_ByteBufferLength - m_ByteBufferIndex;
        if (newLength == 0)
        {
            m_ByteBufferLength = 0;
            m_ByteBufferIndex = 0;
        }
        else if (newLength > 0)
        {
            Array.Copy(m_ByteBuffer, m_ByteBufferIndex, m_ByteBuffer, 0, newLength);
            m_ByteBufferLength = newLength;
            m_ByteBufferIndex = 0;
        }
        else
        {
            Debug.LogError("MoveBytesToHead Failed, Buffer Index Is Larger Than Length");
        }
    }

    public void Reset()
    {
        m_ByteBufferIndex = 0;
        m_ByteBufferLength = 0;
    }
}
