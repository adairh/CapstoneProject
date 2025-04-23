using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private LobbyPopupUI lobbyPopupUI;

    public static LobbyUI Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("LobbyUI Awake");
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate components
        if (mainMenuButton == null) Debug.LogError("MainMenuButton is not assigned!");
        if (quickJoinButton == null) Debug.LogError("QuickJoinButton is not assigned!");
        if (createLobbyButton == null) Debug.LogError("CreateLobbyButton is not assigned!");
        if (lobbyPopupUI == null) Debug.LogError("LobbyPopupUI is not assigned!");
        if (statusText == null) Debug.LogWarning("StatusText is not assigned (optional).");

        mainMenuButton.onClick.AddListener(() =>
        {
            Debug.Log("Main Menu Button Clicked");
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        });

        createLobbyButton.onClick.AddListener(() =>
        {
            Debug.Log("Create Lobby Button Clicked");
            if (lobbyPopupUI == null)
            {
                Debug.LogError("lobbyPopupUI is null when trying to show popup!");
                return;
            }
            lobbyPopupUI.Show((lobbyName, password) =>
            {
                if (GameLobby.Instance != null)
                {
                    GameLobby.Instance.CreateLobby(lobbyName, password, false);
                    UpdateStatus($"Creating lobby: {lobbyName}...");
                }
                else
                {
                    Debug.LogError("GameLobby instance not found!");
                    UpdateStatus("Error: Lobby system not initialized!");
                }
            });
        });

        quickJoinButton.onClick.AddListener(() =>
        {
            Debug.Log("Quick Join Button Clicked");
            if (lobbyPopupUI == null)
            {
                Debug.LogError("lobbyPopupUI is null when trying to show popup!");
                return;
            }
            lobbyPopupUI.Show((lobbyName, password) =>
            {
                if (GameLobby.Instance != null)
                {
                    GameLobby.Instance.JoinLobbyByNameAndPassword(lobbyName, password);
                    UpdateStatus($"Joining lobby: {lobbyName}...");
                }
                else
                {
                    Debug.LogError("GameLobby instance not found!");
                    UpdateStatus("Error: Lobby system not initialized!");
                }
            });
        });
    }

    private void Start()
    {
        Debug.Log("LobbyUI Start");
        gameObject.SetActive(true);
        if (lobbyPopupUI != null)
        {
            lobbyPopupUI.Hide();
        }
        else
        {
            Debug.LogError("lobbyPopupUI is null in Start!");
        }
    }

    public void Hide()
    {
        Debug.Log("Hiding LobbyUI");
        gameObject.SetActive(false);
    }

    public void UpdateStatus(string message)
    {
        Debug.Log($"LobbyUI Status: {message}");
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}