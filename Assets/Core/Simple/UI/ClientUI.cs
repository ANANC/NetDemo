﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class ClientUI : MonoBehaviour
{
    public Button m_ConnectButton;
    public Button m_AuthButton;
    public Button m_SendButton;
    public Text m_Text;
    public InputField m_Inputfield;

    private ClientNetUser m_Net;

    private void Awake()
    {
        m_Net = new ClientNetUser();
        m_Net.m_ContentShow = Show;
    }

    void Start()
    {
        m_ConnectButton.onClick.AddListener(ConnentButtonOnClick);
        m_AuthButton.onClick.AddListener(ClientAuthButtonOnClick);
        m_SendButton.onClick.AddListener(ClientSendButtonOnClick);

    }

    // Update is called once per frame
    void Update()
    {
        m_Net.Update();
    }

    void OnDestroy()
    {
        m_Net.Close();
    }

    public void ConnentButtonOnClick()
    {
        m_Net.Connect(Define.HOST,Define.PORT);
        m_ConnectButton.gameObject.SetActive(false);
        m_AuthButton.gameObject.SetActive(true);

        Show("客户端请求连接");
    }

    public void ClientAuthButtonOnClick()
    {
        m_AuthButton.gameObject.SetActive(false);
        m_SendButton.gameObject.SetActive(true);

        m_Net.SendAuthReq();
    }

    public void ClientSendButtonOnClick()
    {
        string text = m_Inputfield.text;
        m_Net.SendMessage(text);
    }

    public void Show(string content)
    {
        m_Text.text += string.Format("net Client : ( {0} )\n   {1}\n\n", DateTime.Now, content);
    }
}
