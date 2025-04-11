using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    //[SerializeField] private TextMeshProUGUI statusText; 

    private void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            Debug.Log("Starting Host");
            NetworkManager.Singleton.StartHost();
            Hide();
            //statusText.text = "Host Started"; 
        });
        startClientButton.onClick.AddListener(() =>
        {
            Debug.Log("Starting Client");
            NetworkManager.Singleton.StartClient();
            Hide();
            //statusText.text = "Client Started"; 
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}