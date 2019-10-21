
public interface INetPackage
{
    /// <summary>
    /// 指定一个Connect来发送数据
    /// </summary>
    bool SendPackage<T>(IConnection connection, int command, T pack);

    /// <summary>
    /// 读取包中的内容(剔除第一个包长int后的数据)
    /// </summary>
    string ReadPackage(byte[] bytes, int offset, int length);

    /// <summary>
    /// 持续监听数据的缓存池
    /// </summary>
    void Update();

    /// <summary>
    /// 设置读取包体的对象
    /// </summary>
    void SetNetBody(INetBody netBody);
}