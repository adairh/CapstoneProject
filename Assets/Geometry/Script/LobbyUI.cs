using UnityEngine;
using UnityEngine.UI;
using TMPro;
using An_An;
using Khoa;
using Manipulator;
using System.Collections;
using System.Collections.Generic;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private CreatePopupUI createPopupUI;
    [SerializeField] private JoinPopupUI joinPopupUI;
    [SerializeField] private GameObject statusPopupPrefab;
    [SerializeField] private GameObject loadingPanel; // New loading panel

    public static LobbyUI Instance { get; private set; }

    private Canvas uiCanvas;
    private string lastStatusMessage = "";
    private float lastStatusTime = 0f;
    private float statusRepeatCooldown = 3.0f; // Increased to 3 seconds
    private float minLoadingDisplayTime = 5.0f; // Minimum time the loading screen stays visible (in seconds)
    private float loadingStartTime; // Track when loading started
    private Coroutine loadingCoroutine; // Track the loading coroutine
    private bool isShowingPopup = false; // Lock to prevent duplicate pop-ups
    private Queue<string> statusMessageQueue = new Queue<string>(); // Queue for pending messages

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
        if (loadingPanel == null) Debug.LogError("LoadingPanel is not assigned!");

        // Get the Canvas this LobbyUI is attached to
        uiCanvas = GetComponentInParent<Canvas>();
        if (uiCanvas == null)
        {
            Debug.LogError("No Canvas found in the parent hierarchy of LobbyUI!");
        }

        // Ensure loading panel is hidden initially
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
            Debug.Log("Loading panel initialized and hidden.");
        }
        else
        {
            Debug.LogError("LoadingPanel is null during initialization!");
        }

        createLobbyButton.onClick.AddListener(() =>
        {
            Debug.Log("Create Lobby Button Clicked");
            if (createPopupUI == null)
            {
                Debug.LogError("createPopupUI is null when trying to show popup!");
                return;
            }
            createPopupUI.Show((lobbyName, password, isPrivate) =>
            {
                Debug.Log($"User confirmed creation of lobby: {lobbyName}, Private: {isPrivate}");
                if (GameLobby.Instance != null)
                {
                    ShowLoading(); // Show loading after user confirms
                    GameLobby.Instance.CreateLobby(lobbyName, password, isPrivate);
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
                Debug.Log($"User confirmed joining lobby: {lobbyName}");
                if (GameLobby.Instance != null)
                {
                    ShowLoading(); // Show loading after user confirms
                    GameLobby.Instance.JoinLobbyByNameAndPassword(lobbyName, password);
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
        HideLoading(); // Ensure loading panel is hidden when LobbyUI is hidden
    }

    public void ShowLoading()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            loadingStartTime = Time.time; // Record the start time
            Debug.Log("Loading panel shown at: " + loadingStartTime);

            // Start or restart the loading coroutine to enforce minimum display time
            if (loadingCoroutine != null)
            {
                StopCoroutine(loadingCoroutine);
            }
            loadingCoroutine = StartCoroutine(MinLoadingDisplayTime());
        }
        else
        {
            Debug.LogError("LoadingPanel is null when trying to show!");
        }
    }

    private IEnumerator MinLoadingDisplayTime()
    {
        while (Time.time - loadingStartTime < minLoadingDisplayTime)
        {
            yield return null;
        }
        Debug.Log("Minimum loading display time elapsed at: " + Time.time);
        if (loadingPanel != null && loadingPanel.activeSelf)
        {
            loadingPanel.SetActive(false);
            Debug.Log("Loading panel hidden due to minimum time.");
        }
        loadingCoroutine = null;
    }

    public void HideLoading()
    {
        if (loadingPanel != null)
        {
            if (loadingCoroutine != null)
            {
                StopCoroutine(loadingCoroutine);
                loadingCoroutine = null;
            }
            // Bypass minimum time for error messages to hide immediately
            if (lastStatusMessage.StartsWith("Error:"))
            {
                loadingPanel.SetActive(false);
                Debug.Log("Loading panel hidden immediately due to error: " + lastStatusMessage);
            }
            else if (Time.time - loadingStartTime >= minLoadingDisplayTime)
            {
                loadingPanel.SetActive(false);
                Debug.Log("Loading panel hidden after minimum time elapsed.");
            }
            else
            {
                Debug.Log("Loading panel hide delayed, waiting for minimum time.");
            }
        }
        else
        {
            Debug.LogError("LoadingPanel is null when trying to hide!");
        }
    }

    public void UpdateStatus(string message)
    {
        float now = Time.time;

        // Prevent duplicate pop-ups by checking message and cooldown
        if (message == lastStatusMessage && (now - lastStatusTime) < statusRepeatCooldown)
        {
            Debug.Log($"Duplicate status ignored due to cooldown: {message}");
            return;
        }

        // If a popup is already being shown, queue the message
        if (isShowingPopup)
        {
            Debug.Log($"Popup already active, queuing message: {message}");
            statusMessageQueue.Enqueue(message);
            return;
        }

        lastStatusMessage = message;
        lastStatusTime = now;

        Debug.Log($"Status: {message}");
        HideLoading(); // Attempt to hide loading screen when a status update is received

        // Show popup for specific errors: lobby creation failure or join failure
        if ((message.Contains("Lobby '") && message.Contains("already exists!")) || // Creation error
            message.Contains("No lobby named") || // Join error: wrong lobby ID
            message.Contains("Incorrect password")) // Join error: wrong password
        {
            isShowingPopup = true;
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
                // Reset lock when popup starts fading (assuming StatusPopup destroys itself)
                StartCoroutine(ResetPopupLockAfterFade(popup));
            }
            else
            {
                Debug.LogError("StatusPopup component not found!");
                Destroy(notification);
                isShowingPopup = false;
            }
        }
    }

    private IEnumerator ResetPopupLockAfterFade(StatusPopup popup)
    {
        yield return new WaitForSeconds(popup.displayTime + popup.fadeTime); // Wait for full display and fade
        isShowingPopup = false;
        Debug.Log("Popup lock reset");

        // Process next message in queue, if any
        if (statusMessageQueue.Count > 0)
        {
            string nextMessage = statusMessageQueue.Dequeue();
            Debug.Log($"Processing queued message: {nextMessage}");
            UpdateStatus(nextMessage);
        }
    }
}