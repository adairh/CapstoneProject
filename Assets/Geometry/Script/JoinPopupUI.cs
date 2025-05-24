using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

using An_An;
using Manipulator;

public class JoinPopupUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject notificationPanelPrefab; // Changed to prefab reference

    private Canvas uiCanvas;
    private Action<string, string> onConfirm;

    private void Awake()
    {
        Debug.Log("JoinPopupUI Awake - Checking components...");
        
        if (lobbyNameInputField == null) Debug.LogError("LobbyNameInputField is not assigned in JoinPopupUI!");
        if (passwordInputField == null) Debug.LogError("PasswordInputField is not assigned in JoinPopupUI!");
        if (confirmButton == null) Debug.LogError("ConfirmButton is not assigned in JoinPopupUI!");
        if (cancelButton == null) Debug.LogError("CancelButton is not assigned in JoinPopupUI!");
        if (notificationPanelPrefab == null) Debug.LogError("NotificationPanelPrefab is not assigned in JoinPopupUI!");

        // Get the canvas reference
        uiCanvas = GetComponentInParent<Canvas>();
        if (uiCanvas == null)
        {
            Debug.LogError("No Canvas found in the parent hierarchy of JoinPopupUI!");
        }

        confirmButton.onClick.AddListener(() =>
        {
            Debug.Log("Join button clicked - Validating inputs...");
            string lobbyName = lobbyNameInputField.text?.Trim();
            string password = passwordInputField.text?.Trim();

            Debug.Log($"Input values - LobbyName: '{lobbyName}', Password: '{password}'");

            // Validate input fields with specific messages
            if (string.IsNullOrEmpty(lobbyName) && string.IsNullOrEmpty(password))
            {
                Debug.Log("Both fields are empty - showing notification");
                ShowNotification("Please enter both lobby name and password!");
                return;
            }
            else if (string.IsNullOrEmpty(lobbyName))
            {
                Debug.Log("Lobby name is empty - showing notification");
                ShowNotification("Please enter a lobby name!");
                return;
            }
            else if (string.IsNullOrEmpty(password))
            {
                Debug.Log("Password is empty - showing notification");
                ShowNotification("Please enter a password!");
                return;
            }

            Debug.Log("Input validation passed - proceeding with lobby join");
            onConfirm?.Invoke(lobbyName, password);
            
            Debug.Log("Confirm Clicked: Checking GameLobby.Instance...");
            if (GameLobby.Instance == null)
            {
                ShowNotification("Game lobby system is not available!");
                Debug.LogError("GameLobby.Instance is NULL!");
            }
            else
            {
                Debug.Log("GameLobby.Instance is valid: " + GameLobby.Instance);
                GameLobby.Instance.JoinLobbyByNameAndPassword(lobbyName, password);
                Hide();
            }
        });

        cancelButton.onClick.AddListener(() =>
        {
            Debug.Log("Cancel button clicked");
            Hide();
            if (darkOverlay != null) darkOverlay.SetActive(false);
        });
    }

    private void ShowNotification(string message)
    {
        Debug.Log($"Attempting to show notification: {message}");
        
        if (notificationPanelPrefab == null)
        {
            Debug.LogError("Notification panel prefab is null in ShowNotification!");
            return;
        }

        if (uiCanvas == null)
        {
            Debug.LogError("No Canvas found for notification!");
            return;
        }

        // Instantiate the notification panel
        GameObject notification = Instantiate(notificationPanelPrefab, Vector3.zero, Quaternion.identity);
        notification.transform.SetParent(uiCanvas.transform, false);

        // Position the notification
        RectTransform rect = notification.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 50); // Position at top center
        rect.localScale = Vector3.one;

        // Get and set the text
        TextMeshProUGUI notificationText = notification.GetComponentInChildren<TextMeshProUGUI>(true);
        if (notificationText != null)
        {
            notificationText.text = message;
            Debug.Log("Notification text set and panel shown");
        }
        else
        {
            Debug.LogError("No TextMeshProUGUI component found in notification panel prefab!");
            Destroy(notification);
            return;
        }

        // Add StatusPopup component if it doesn't exist
        StatusPopup statusPopup = notification.GetComponent<StatusPopup>();
        if (statusPopup == null)
        {
            statusPopup = notification.AddComponent<StatusPopup>();
            statusPopup.statusText = notificationText;
        }

        // Use the StatusPopup component to handle the fade
        statusPopup.SetStatus(message);
    }

    public void Show(Action<string, string> confirmCallback)
    {
        Debug.Log("Showing JoinPopupUI");
        gameObject.SetActive(true);
        lobbyNameInputField.text = "";
        passwordInputField.text = "";
        onConfirm = confirmCallback;
    }

    public void Hide()
    {
        Debug.Log("Hiding JoinPopupUI");
        gameObject.SetActive(false);
    }
}
