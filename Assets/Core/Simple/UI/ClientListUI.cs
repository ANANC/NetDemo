using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientListUI : MonoBehaviour
{
    public Button m_AddButton;
    public ClientUI m_Client;
    public RectTransform m_Content;

    private float m_ClientHeight;
    private List<GameObject> m_ClientPanelList = new List<GameObject>();

    void Start()
    {
        m_ClientHeight = m_Client.GetComponent<RectTransform>().sizeDelta.y+10;
        m_AddButton.onClick.AddListener(AddClient);
    }

    public void AddClient()
    {
        Transform panel = GameObject.Instantiate(m_Client.gameObject).transform;
        panel.SetParent(m_Content);
        m_ClientPanelList.Add(panel.gameObject);
        panel.gameObject.SetActive(true);
        m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x , m_ClientHeight * m_ClientPanelList.Count);
    }
}
