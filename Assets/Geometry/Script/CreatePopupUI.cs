using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

using An_An;
using Manipulator;

public class CreatePopupUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject darkOverlay; // Optional: for dimming background when popup is active

    private Action<string, string> onConfirm;
    private void Awake()
    {
        if (lobbyNameInputField == null) Debug.LogError("LobbyNameInputField is not assigned in LobbyPopupUI!");
        if (passwordInputField == null) Debug.LogError("PasswordInputField is not assigned in LobbyPopupUI!");
        if (confirmButton == null) Debug.LogError("ConfirmButton is not assigned in LobbyPopupUI!");
        if (cancelButton == null) Debug.LogError("CancelButton is not assigned in LobbyPopupUI!");

        confirmButton.onClick.AddListener(() =>
        {
            string lobbyName = lobbyNameInputField.text?.Trim();
            string password = passwordInputField.text?.Trim();
            Debug.Log($"Popup Input: Name={lobbyName}, Password={password}");
            if (string.IsNullOrEmpty(lobbyName) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("Lobby name and password cannot be empty!");
                return;
            }
            onConfirm?.Invoke(lobbyName, password);
            Debug.Log("Confirm Clicked: Checking GameLobby.Instance...");
            if (GameLobby.Instance == null)
            {
                Debug.LogError("GameLobby.Instance is NULL!");
            }
            else
            {
                Debug.Log("GameLobby.Instance is valid: " + GameLobby.Instance);
                GameLobby.Instance.CreateLobby(lobbyName, password, false);
            }
            Hide();
        });

        cancelButton.onClick.AddListener(() =>
        {
            Hide();
            if (darkOverlay != null) darkOverlay.SetActive(false);
            //SceneManager.LoadScene("MAIN");
        });
    }

    public void Show(Action<string, string> confirmCallback)
    {
        gameObject.SetActive(true);
        lobbyNameInputField.text = "";
        passwordInputField.text = "";
        onConfirm = confirmCallback;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
