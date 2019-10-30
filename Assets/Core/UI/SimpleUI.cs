using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleUI : MonoBehaviour
{
    public NetTestMgr m_NetManager;

    public Button m_ConnectButton;
    public Button m_AuthButton;

    public InputField m_ClientInputfield;
    public Button m_ClientSendButton;

    public Text m_ClientText;
    public Text m_ServerText;

    void Start()
    {
        NetTestMgr.ShowStrContentEvent += Show;

        m_ServerText.text = string.Empty;
        m_ClientText.text = string.Empty;


        m_ConnectButton.onClick.AddListener(ConnentButtonOnClick);
        m_AuthButton.onClick.AddListener(ClientAuthButtonOnClick);
        m_ClientSendButton.onClick.AddListener(ClientSendButtonOnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConnentButtonOnClick()
    {
        m_NetManager.ClientContent();
        m_ConnectButton.gameObject.SetActive(false);
        m_AuthButton.gameObject.SetActive(true);
    }
    public void ClientAuthButtonOnClick()
    {
        m_AuthButton.gameObject.SetActive(false);

        NetTestMgr.ShowStrContentEvent(false, "客户端请求认证");

        Msg.C2G.AuthReq pack = new Msg.C2G.AuthReq();
        m_NetManager.ClientSend<Msg.C2G.AuthReq>((int)Msg.C2G.CMD.AuthReq, pack);
    }

    public void ClientSendButtonOnClick()
    {
        string text = m_ClientInputfield.text;
        m_NetManager.ClientSend(text);
    }

    public void Show(bool isServer, string Content)
    {
        if (isServer)
        {
            m_ServerText.text += string.Format("net Serve : ( {0} )\n   {1}\n\n", DateTime.Now, Content);
        }
        else
        {
            m_ClientText.text += string.Format("net Client : ( {0} )\n   {1}\n\n", DateTime.Now, Content);
        }

        Debug.Log(Content);
    }
}
