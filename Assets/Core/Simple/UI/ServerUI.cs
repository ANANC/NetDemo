using System;
using UnityEngine;
using UnityEngine.UI;

public class ServerUI : MonoBehaviour
{
    public Text m_Text;
    public RectTransform m_TextMask;

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
        m_Text.text += string.Format("{0}\n\n",content);
        UpdateTextPosition();
    }

    private void UpdateTextPosition()
    {
        Vector3 pos = new Vector3(0, (m_Text.rectTransform.sizeDelta.y - m_TextMask.sizeDelta.y) / 2);
        m_Text.transform.localPosition = pos;
    }
}
