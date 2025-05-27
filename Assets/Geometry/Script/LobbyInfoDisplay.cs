using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInfoDisplay : MonoBehaviour
{
    [SerializeField] private Button showInfoButton; // Button to show the lobby info window
    [SerializeField] private GameObject infoWindow; // The UI panel for the lobby info window
    [SerializeField] private TextMeshProUGUI lobbyIdText; // Text field for Lobby ID
    [SerializeField] private TextMeshProUGUI passwordText; // Text field for Password
    [SerializeField] private Button copyButton; // Button to copy to clipboard
    [SerializeField] private Button closeButton; // Button to close the window
    [SerializeField] private TextMeshProUGUI statusText; // Button to close the window

    private GameLobby gameLobby; // Reference to GameLobby

    private void Awake()
    {
        gameLobby = FindObjectOfType<GameLobby>();
        if (gameLobby == null)
        {
            Debug.LogError("GameLobby not found in the scene!");
            return;
        }

        // Ensure the info window is hidden at start
        if (infoWindow != null) infoWindow.SetActive(false);

        // Add listeners
        if (showInfoButton != null) showInfoButton.onClick.AddListener(ShowLobbyInfo);
        if (closeButton != null) closeButton.onClick.AddListener(HideInfoWindow);
        if (copyButton != null) copyButton.onClick.AddListener(CopyLobbyInfoToClipboard);
    }

    private void ShowLobbyInfo()
    {
        if (gameLobby == null || !gameLobby.IsHost)
        {
            Debug.LogWarning("Only the host can view lobby information.");
            return;
        }

        if (gameLobby.GetJoinedLobby() == null)
        {
            Debug.LogWarning("No lobby is currently joined.");
            return;
        }

        var lobbyName = gameLobby.GetJoinedLobby().Name;
        var password = gameLobby.GetJoinedLobby().Data != null &&
                       gameLobby.GetJoinedLobby().Data.TryGetValue("Password", out var passwordData)
            ? passwordData.Value
            : "No Password";

        if (infoWindow != null && lobbyIdText != null && passwordText != null)
        {
            lobbyIdText.text = $"Lobby ID: {lobbyName}";
            passwordText.text = $"Password: {password}";
            infoWindow.SetActive(true);
            Debug.Log("Lobby info window shown.");
        }
        else
        {
            Debug.LogError("Lobby info window components are not assigned!");
        }
    }

    private void HideInfoWindow()
    {
        if (infoWindow != null)
        {
            infoWindow.SetActive(false);
            Debug.Log("Lobby info window hidden.");
        }
    }

    private void CopyLobbyInfoToClipboard()
    {
        if (gameLobby == null || gameLobby.GetJoinedLobby() == null)
        {
            Debug.LogWarning("No lobby to copy info from.");
            return;
        }

        var lobbyName = gameLobby.GetJoinedLobby().Name;
        var password = gameLobby.GetJoinedLobby().Data != null &&
                       gameLobby.GetJoinedLobby().Data.TryGetValue("Password", out var passwordData)
            ? passwordData.Value
            : "No Password";

        var clipboardText = $"Lobby ID: {lobbyName}\nPassword: {password}";
        GUIUtility.systemCopyBuffer = clipboardText;
        Debug.Log($"Copied to clipboard: {clipboardText}");
        statusText.text = "Successfully Copied!";
    }
}