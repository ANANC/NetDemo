using System;
using UnityEngine;
using UnityEngine.UI;

public class ServerUI : MonoBehaviour
{
    public Text m_Text;

    private ServerNetUser m_Net;

    void Start()
    {
        m_Net = new ServerNetUser(Define.PORT);
        m_Net.m_ContentShow = Show;
        Show("服务器开启");
    }

    void Update()
    {
        m_Net.Update();
    }

    private void OnDestroy()
    {
        m_Net.Close();
    }

    public void Show(string content)
    {
        m_Text.text += string.Format("net Server : ( {0} )\n   {1}\n\n", DateTime.Now, content);
    }
}
