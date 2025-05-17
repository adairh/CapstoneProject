using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatePopupUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Toggle privateToggle; // New Toggle for private/public setting

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject darkOverlay; // Optional: for dimming background when popup is active

    private Action<string, string, bool> onConfirm; // Updated callback to include isPrivate

    private void Awake()
    {
        if (lobbyNameInputField == null) Debug.LogError("LobbyNameInputField is not assigned in CreatePopupUI!");
        if (passwordInputField == null) Debug.LogError("PasswordInputField is not assigned in CreatePopupUI!");
        if (privateToggle == null) Debug.LogError("PrivateToggle is not assigned in CreatePopupUI!");
        if (confirmButton == null) Debug.LogError("ConfirmButton is not assigned in CreatePopupUI!");
        if (cancelButton == null) Debug.LogError("CancelButton is not assigned in CreatePopupUI!");

        confirmButton.onClick.AddListener(() =>
        {
            var lobbyName = lobbyNameInputField.text?.Trim();
            var password = passwordInputField.text?.Trim();
            var isPrivate = privateToggle.isOn; // Get the state of the toggle (true = private, false = public)
            Debug.Log($"Popup Input: Name={lobbyName}, Password={password}, IsPrivate={isPrivate}");
            if (string.IsNullOrEmpty(lobbyName) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("Lobby name and password cannot be empty!");
                return;
            }

            onConfirm?.Invoke(lobbyName, password, isPrivate);
            Debug.Log("Confirm Clicked: Checking GameLobby.Instance...");
            if (GameLobby.Instance == null)
            {
                Debug.LogError("GameLobby.Instance is NULL!");
            }
            else
            {
                Debug.Log("GameLobby.Instance is valid: " + GameLobby.Instance);
                GameLobby.Instance.CreateLobby(lobbyName, password, isPrivate);
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

    public void Show(Action<string, string, bool> confirmCallback)
    {
        gameObject.SetActive(true);
        lobbyNameInputField.text = "";
        passwordInputField.text = "";
        privateToggle.isOn = false; // Default to public (unchecked)
        onConfirm = confirmCallback;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}