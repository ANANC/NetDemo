using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientListUI : MonoBehaviour
{
    public Button m_AddButton;
    public Button m_ClientButton;
    public Transform m_ClientBtnList;
    public ClientUI m_Client;
    public Transform m_ClientList;

    private List<Button> m_ClientButtonList = new List<Button>();
    private List<GameObject> m_ClientPanelList = new List<GameObject>();

    void Start()
    {
        m_AddButton.onClick.AddListener(AddClient);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddClient()
    {
        Transform btn = GameObject.Instantiate(m_ClientButton.gameObject).transform;
        btn.parent = m_ClientBtnList;
        m_ClientButtonList.Add(btn.GetComponent<Button>());

        Transform panel = GameObject.Instantiate(m_Client.gameObject).transform;
        panel.parent = m_ClientList;
        m_ClientPanelList.Add(panel.gameObject);
    }
}
