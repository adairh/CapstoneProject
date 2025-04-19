using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TextMeshProUGUI statusText;

    public static LobbyUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate UI components
        /*if (mainMenuButton == null) Debug.LogError("MainMenuButton is not assigned!");
        if (quickJoinButton == null) Debug.LogError("QuickJoinButton is not assigned!");
        if (createLobbyButton == null) Debug.LogError("CreateLobbyButton is not assigned!");
        if (lobbyNameInputField == null) Debug.LogError("LobbyNameInputField is not assigned!");
        if (statusText == null) Debug.LogWarning("StatusText is not assigned (optional).");*/

        // Button listeners
        mainMenuButton.onClick.AddListener(() =>
        {
            Debug.Log("Main Menu Button Clicked");
            // Optional: Load a main menu scene if you have one
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        });

        quickJoinButton.onClick.AddListener(() =>
        {
            Debug.Log("Quick Join Button Clicked");
            if (Lobby.Instance != null)
            {
                Lobby.Instance.QuickJoinLobby();
            }
            else
            {
                Debug.LogError("Lobby instance not found!");
            }
        });

        createLobbyButton.onClick.AddListener(() =>
        {
            Debug.Log("Create Lobby Button Clicked");
            if (Lobby.Instance == null)
            {
                Debug.LogError("Lobby instance not found!");
                return;
            }
            if (lobbyNameInputField == null)
            {
                Debug.LogError("LobbyNameInputField is null! Assign it in the Inspector.");
                return;
            }

            string lobbyName = string.IsNullOrEmpty(lobbyNameInputField.text) ? "DrawingLobby" : lobbyNameInputField.text;
            Lobby.Instance.CreateLobby(lobbyName, false); // Public lobby
        });
    }

    private void Start()
    {
        // Ensure the UI is visible when the scene starts
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        Debug.Log("Hiding LobbyUI");
        gameObject.SetActive(false);
    }
}