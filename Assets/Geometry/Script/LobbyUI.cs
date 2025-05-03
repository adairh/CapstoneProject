using UnityEngine;
using UnityEngine.UI;
using TMPro;
using An_An;
using Khoa;
using Manipulator;


public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private CreatePopupUI createPopupUI;
    [SerializeField] private JoinPopupUI joinPopupUI;
    [SerializeField] private GameObject statusPopupPrefab;

    public static LobbyUI Instance { get; private set; }

    private Canvas uiCanvas;
    private string lastStatusMessage = "";
    private float lastStatusTime = 0f;
    private float statusRepeatCooldown = 1.0f; // seconds
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
        if (quickJoinButton == null) Debug.LogError("QuickJoinButton is not assigned!");
        if (createLobbyButton == null) Debug.LogError("CreateLobbyButton is not assigned!");
        if (statusText == null) Debug.LogError("StatusText is not assigned!");
        if (createPopupUI == null) Debug.LogError("createPopupUI is not assigned!");
        if (joinPopupUI == null) Debug.LogError("joinPopupUI is not assigned!");
        if (statusPopupPrefab == null) Debug.LogError("StatusPopupPrefab is not assigned!");

        // Get the Canvas this LobbyUI is attached to
        uiCanvas = GetComponentInParent<Canvas>();
        if (uiCanvas == null)
        {
            Debug.LogError("No Canvas found in the parent hierarchy of LobbyUI!");
        }

        createLobbyButton.onClick.AddListener(() =>
        {
            Debug.Log("Create Lobby Button Clicked");
            if (createPopupUI == null)
            {
                Debug.LogError("createPopupUI is null when trying to show popup!");
                return;
            }
            createPopupUI.Show((lobbyName, password) =>
            {
                if (GameLobby.Instance != null)
                {
                    GameLobby.Instance.CreateLobby(lobbyName, password, false);
                    UpdateStatus($"Creating lobby: {lobbyName}...");
                }
                else
                {
                    Debug.LogError("GameLobby instance not found!");
                    UpdateStatus("Lobby system not initialized!");
                }
            });
        });

        quickJoinButton.onClick.AddListener(() =>
        {
            Debug.Log("Quick Join Button Clicked");
            if (joinPopupUI == null)
            {
                Debug.LogError("joinPopupUI is null when trying to show popup!");
                return;
            }
            joinPopupUI.Show((lobbyName, password) =>
            {
                if (GameLobby.Instance != null)
                {
                    GameLobby.Instance.JoinLobbyByNameAndPassword(lobbyName, password);
                    UpdateStatus($"Joining lobby: {lobbyName}...");
                }
                else
                {
                    Debug.LogError("GameLobby instance not found!");
                    UpdateStatus("Lobby system not initialized!");
                }
            });
        });
    }

    private void Start()
    {
        Debug.Log("Lobby Start");
        gameObject.SetActive(true);
        if (createPopupUI != null)
        {
            createPopupUI.Hide();
        }
        else
        {
            Debug.LogError("createPopupUI is null in Start!");
        }

        if (joinPopupUI != null)
        {
            joinPopupUI.Hide();
        }
        else
        {
            Debug.LogError("joinPopupUI is null in Start!");
        }
    }

    public void Hide()
    {
        Debug.Log("Hiding LobbyUI");
        gameObject.SetActive(false);
    }

    public void UpdateStatus(string message)
    {
        float now = Time.time;

        // Prevent duplicate popups within the cooldown window
        if (message == lastStatusMessage && (now - lastStatusTime) < statusRepeatCooldown)
        {
            Debug.Log($"Duplicate status message ignored: {message}");
            return;
        }

        lastStatusMessage = message;
        lastStatusTime = now;

        Debug.Log($"Status: {message}");

        // Show popup only for lobby errors
        if (message.Contains("Lobby '") && message.Contains("already exists!"))
        {
            GameObject notification = Instantiate(statusPopupPrefab, Vector3.zero, Quaternion.identity);
            notification.transform.SetParent(uiCanvas.transform, false);

            RectTransform rect = notification.GetComponent<RectTransform>();
            int activePopups = uiCanvas.transform.childCount - 1;
            float yOffset = -50 * activePopups;
            rect.anchoredPosition = new Vector2(0, 50 + yOffset);
            rect.localScale = Vector3.one;

            StatusPopup popup = notification.GetComponent<StatusPopup>();
            if (popup != null)
            {
                popup.SetStatus(message);
            }
            else
            {
                Debug.LogError("StatusPopup component not found!");
                Destroy(notification);
            }
        }
    }


    /*public void UpdateStatus(string message)
    {
        Debug.Log($"Status: {message}");

        // Check if the message is about an existing lobby
        if (message.Contains("Lobby '") && message.Contains("already exists!"))
        {
            if (statusPopupPrefab == null)
            {
                Debug.LogError("StatusPopupPrefab is not assigned in LobbyUI!");
                return;
            }

            if (uiCanvas == null)
            {
                Debug.LogError("uiCanvas is null! Ensure LobbyUI is under a Canvas.");
                return;
            }

            GameObject notification = Instantiate(statusPopupPrefab, Vector3.zero, Quaternion.identity);
            notification.transform.SetParent(uiCanvas.transform, false);
            RectTransform rect = notification.GetComponent<RectTransform>();
            int activePopups = uiCanvas.transform.childCount - 1; // Subtract 1 to exclude LobbyUI itself
            float yOffset = -50 * activePopups;
            rect.anchoredPosition = new Vector2(0, 50 + yOffset);
            rect.localScale = Vector3.one;
            Debug.Log($"Status notification parented to LobbyUI's Canvas at position (0, {50 + yOffset})");

            StatusPopup popup = notification.GetComponent<StatusPopup>();
            if (popup != null)
            {
                popup.SetStatus(message);
            }
            else
            {
                Debug.LogError("StatusPopup component not found on the status notification prefab!");
                Destroy(notification);
            }
        }
        else
        {
            // Log other messages to console only
            Debug.Log(message);
        }
    }*/
}

/*public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private CreatePopupUI createPopupUI;
    [SerializeField] private JoinPopupUI joinPopupUI;
    [SerializeField] private GameObject notificationPrefab;

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
        if (quickJoinButton == null) Debug.LogError("QuickJoinButton is not assigned!");
        if (createLobbyButton == null) Debug.LogError("CreateLobbyButton is not assigned!");
        if (statusText == null) Debug.LogError("StatusText is not assigned!");
        if (createPopupUI == null) Debug.LogError("createPopupUI is not assigned!");
        if (joinPopupUI == null) Debug.LogError("joinPopupUI is not assigned!");
        if (notificationPrefab == null) Debug.LogError("NotificationPrefab is not assigned!");

        createLobbyButton.onClick.AddListener(() =>
        {
            Debug.Log("Create Lobby Button Clicked");
            if (createPopupUI == null)
            {
                Debug.LogError("createPopupUI is null when trying to show popup!");
                return;
            }
            createPopupUI.Show((lobbyName, password) =>
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
            if (joinPopupUI == null)
            {
                Debug.LogError("joinPopupUI is null when trying to show popup!");
                return;
            }
            joinPopupUI.Show((lobbyName, password) =>
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
        Debug.Log("Lobby Start");
        gameObject.SetActive(true);
        if (createPopupUI != null)
        {
            createPopupUI.Hide();
        }
        else
        {
            Debug.LogError("createPopupUI is null in Start!");
        }

        if (joinPopupUI != null)
        {
            joinPopupUI.Hide();
        }
        else
        {
            Debug.LogError("joinPopupUI is null in Start!");
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

        // Instantiate a new notification pop-up
        if (notificationPrefab == null)
        {
            Debug.LogError("NotificationPrefab is not assigned in LobbyUI!");
            return;
        }

        GameObject notification = Instantiate(notificationPrefab, Vector3.zero, Quaternion.identity);
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            notification.transform.SetParent(canvas.transform, false);
            RectTransform rect = notification.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 50);
            rect.localScale = Vector3.one;
            Debug.Log("Status notification parented to Canvas");
        }
        else
        {
            Debug.LogError("No Canvas found in the scene to parent the status notification!");
            Destroy(notification);
            return;
        }

        NotificationPopup popup = notification.GetComponent<NotificationPopup>();
        if (popup != null)
        {
            popup.SetMessage(message); // Updated from SetPlayerId to SetMessage
        }
        else
        {
            Debug.LogError("NotificationPopup component not found on the status notification prefab!");
            Destroy(notification);
        }
    }
}*/