using UnityEngine;
using UnityEngine.UI;
using TMPro;
using An_An;
using Khoa;
using Manipulator;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Geometry;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private CreatePopupUI createPopupUI;
    [SerializeField] private JoinPopupUI joinPopupUI;
    [SerializeField] private GameObject statusPopupPrefab;
    [SerializeField] private GameObject loadingPanel;

    public static LobbyUI Instance { get; private set; }

    private Canvas uiCanvas;
    private string lastStatusMessage = "";
    private float lastStatusTime = 0f;
    private float statusRepeatCooldown = 3.0f;
    private float minLoadingDisplayTime = 8.0f;
    private float loadingStartTime;
    private Coroutine loadingCoroutine;
    private bool isShowingPopup = false;
    private Queue<string> statusMessageQueue = new Queue<string>();

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

        if (quickJoinButton == null) Debug.LogError("QuickJoinButton is not assigned!");
        if (createLobbyButton == null) Debug.LogError("CreateLobbyButton is not assigned!");
        if (statusText == null) Debug.LogError("StatusText is not assigned!");
        if (createPopupUI == null) Debug.LogError("createPopupUI is not assigned!");
        if (joinPopupUI == null) Debug.LogError("joinPopupUI is not assigned!");
        if (statusPopupPrefab == null) Debug.LogError("StatusPopupPrefab is not assigned!");
        if (loadingPanel == null) Debug.LogError("LoadingPanel is not assigned!");

        uiCanvas = GetComponentInParent<Canvas>();
        if (uiCanvas == null)
        {
            Debug.LogError("No Canvas found in the parent hierarchy of LobbyUI!");
        }

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
                    ShowLoading();
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
                    ShowLoading();
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
        
        
        Debug.LogWarning("Lobby UI:" + SceneFlag.IsRandom);
        if (SceneFlag.IsRandom)
        {
            CallDelayedLobbyCreation();
            SceneFlag.IsRandom = false;
            return;
        }

    }
 
    private async void CallDelayedLobbyCreation()
    {
        ShowLoading();             
        await Task.Delay(1000);                         
        await GameLobby.Instance.CreateRandomLobby(); 
        HideLoading();             
    }

    
    public void Hide()
    {
        Debug.Log("Hiding LobbyUI");
        gameObject.SetActive(false);
        HideLoading();
    }

    public void ShowLoading()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            loadingStartTime = Time.time;
            Debug.Log("Loading panel shown at: " + loadingStartTime);

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
            // Hide loading panel only for errors that are not rate limit related
            if (lastStatusMessage.StartsWith("Error:") && !lastStatusMessage.Contains("Rate limit has been exceeded"))
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

        if (message == lastStatusMessage && (now - lastStatusTime) < statusRepeatCooldown)
        {
            Debug.Log($"Duplicate status ignored due to cooldown: {message}");
            return;
        }

        if (isShowingPopup)
        {
            Debug.Log($"Popup already active, queuing message: {message}");
            statusMessageQueue.Enqueue(message);
            return;
        }

        lastStatusMessage = message;
        lastStatusTime = now;

        Debug.Log($"Status: {message}");

        // Hide loading panel only on success or final error (excluding rate limit errors)
        if (message == "Host Started" || message == "Client Connected" ||
            (message.StartsWith("Error:") && !message.Contains("Rate limit has been exceeded")))
        {
            HideLoading();
        }

        if ((message.Contains("Lobby '") && message.Contains("already exists!")) ||
            message.Contains("No lobby named") ||
            message.Contains("Incorrect password") ||
            message.Contains("Failed to create lobby after"))
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
        yield return new WaitForSeconds(popup.displayTime + popup.fadeTime);
        isShowingPopup = false;
        Debug.Log("Popup lock reset");

        if (statusMessageQueue.Count > 0)
        {
            string nextMessage = statusMessageQueue.Dequeue();
            Debug.Log($"Processing queued message: {nextMessage}");
            UpdateStatus(nextMessage);
        }
    }
}