public interface INetBody
{
    /// <summary>
    /// 读取包体内容
    /// </summary>
    string ReadBody(byte[] bytes, int offset, int length);

    /// <summary>
    /// 发送包体内容
    /// </summary>
    bool SendBody<T>(T pack, int command, byte[] bytes, ref int length);

    /// <summary>
    /// 在主线程中执行解析后的内容
    /// </summary>
    void Excute();
}