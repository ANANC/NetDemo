using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


/// <summary>
/// 定义网络断开的类型
/// </summary>
public enum ConnectNotificationType
{
    Connected,
    Disconnect,
    Exception
}

public static class ClientNetUtils
{
    //写入格式，和服务器要匹配
    public static void WriteIntToBytes(int value, byte[] bytes, ref int offset)
    {
        bytes[offset + 3] = (byte)((value >> 24));
        bytes[offset + 2] = (byte)((value >> 16));
        bytes[offset + 1] = (byte)((value >> 8));
        bytes[offset + 0] = (byte)((value));
        offset += 4;
    }

    //向指定偏移的位写入数据
    public static void WriteByteToBytes(byte value, byte[] bytes, ref int offset)
    {
        bytes[offset] = value;
        offset++;
    }

    //读取格式，和服务器要匹配
    public static int ReadIntFromBytes(byte[] bytes, ref int offset)
    {
        int value;
        value = (int)((bytes[offset + 0])
                   | ((bytes[offset + 1]) << 8)
                   | ((bytes[offset + 2]) << 16)
                   | ((bytes[offset + 3]) << 24));
        offset += 4;
        return value;
    }


    //从指定偏移位读取数据
    public static byte ReadByteFromBytes(byte[] bytes, ref int offset)
    {
        byte value = bytes[offset];
        offset++;
        return value;
    }

    /// <summary>
    /// 获取IP地址，支持IPV4和IPV6网络环境
    /// </summary>
    public static IPAddress GetIpAddress(string host)
    {
        IPAddress addr = null;
        IPHostEntry entry = null;
        try
        {
            entry = Dns.GetHostEntry(host);
            if (entry != null && entry.AddressList.Length > 0)
            {
                Debug.LogWarning(string.Format(">>>>>>>>>>>>>>>> address list length = {0}", entry.AddressList.Length));
                foreach (var ip in entry.AddressList)
                {
                    Debug.LogWarning(string.Format(">>>>>>>>>>>>>>>> host ip is {0}, family type is {1}", ip.ToString(), ip.AddressFamily));
                    if (ip.AddressFamily == AddressFamily.InterNetworkV6 || addr == null)
                    {
                        addr = ip;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        return addr;
    }
}
