using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleUI : MonoBehaviour
{
    public NetManager m_NetManager;

    public Button m_ConnectButton;

    public InputField m_ClientInputfield;
    public Button m_ClientSendButton;

    public Text m_ClientText;
    public Text m_ServerText;

    void Start()
    {
        m_ConnectButton.onClick.AddListener(ConnentButtonOnClick);
        m_ClientSendButton.onClick.AddListener(ClientSendButtonOnClick); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConnentButtonOnClick()
    {
        m_NetManager.ClientContent();
    }

    public void ClientSendButtonOnClick()
    {
        string text = m_ClientInputfield.text;
        m_NetManager.ClientSend(text);
    }
}
