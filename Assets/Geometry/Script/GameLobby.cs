using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using System.Linq;
using An_An;
using Manipulator;
using Khoa;
/// <summary>
/// By Khoa
/// </summary>
public class GameLobby : MonoBehaviour
{
    public static GameLobby Instance { get; private set; }
    public const int max_user_amount = 40;
    private Unity.Services.Lobbies.Models.Lobby joinedLobby;
    private string profileId;
    private Dictionary<ulong, string> clientIdToPlayerIdMap = new Dictionary<ulong, string>();

    [SerializeField] private GameObject notificationPrefab;

    private bool hasReportedError = false; // Flag to prevent duplicate error reports

    private void Awake()
    {
        Debug.Log("GameLobby Awake - This instance: " + this);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameLobby Instance set successfully.");
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                Debug.Log("OnClientConnectedCallback registered in Awake");
            }
            else
            {
                Debug.LogWarning("NetworkManager.Singleton is null in Awake!");
            }
            InitializeUnityAuthentication();
        }
        else
        {
            Debug.LogWarning("GameLobby already exists, destroying duplicate!");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (IsHost)
        {
            InvokeRepeating(nameof(SendHeartbeat), 10f, 10f);
        }
    }

    private bool IsHost
    {
        get
        {
            return NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        }
    }

    private async void OnClientConnected(ulong clientId)
    {
        Debug.Log($"OnClientConnected called with Client ID: {clientId}, IsHost: {IsHost}");
        if (IsHost)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log($"Skipping notification for host's Client ID: {clientId}");
                return;
            }

            string playerId = await GetPlayerIdFromClientId(clientId);
            if (string.IsNullOrEmpty(playerId))
            {
                Debug.LogWarning($"Could not find PlayerId for Client ID: {clientId}");
                playerId = $"Unknown_{clientId}";
            }

            clientIdToPlayerIdMap[clientId] = playerId;
            Debug.Log($"Client connected with ID: {clientId}, PlayerId: {playerId}");

            ShowNotification($"Client Joined: {playerId}");
        }
    }

    private void ShowNotification(string message)
    {
        if (notificationPrefab == null)
        {
            Debug.LogError("NotificationPrefab is not assigned in GameLobby!");
            return;
        }

        GameObject notification = Instantiate(notificationPrefab, Vector3.zero, Quaternion.identity);
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            notification.transform.SetParent(canvas.transform, false);
            RectTransform rect = notification.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 100);
            rect.localScale = Vector3.one;
            Debug.Log("User joined notification parented to Canvas at position (0, 100)");
        }
        else
        {
            Debug.LogError("No Canvas found in the scene to parent the notification!");
            Destroy(notification);
            return;
        }

        NotificationPopup popup = notification.GetComponent<NotificationPopup>();
        if (popup != null)
        {
            popup.SetMessage(message);
        }
        else
        {
            Debug.LogError("NotificationPopup component not found on the notification prefab!");
            Destroy(notification);
        }
    }

    private async Task<string> GetPlayerIdFromClientId(ulong clientId)
    {
        if (joinedLobby == null || joinedLobby.Players == null)
        {
            Debug.LogWarning("Joined lobby or players list is null");
            return null;
        }

        Debug.Log($"Waiting for client to update lobby data for Client ID: {clientId}...");
        await Task.Delay(3000);

        try
        {
            joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            Debug.Log($"Refreshed lobby, player count: {joinedLobby.Players.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to refresh lobby: {e.Message}");
            return null;
        }

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            foreach (var player in joinedLobby.Players)
            {
                if (player.Data != null && player.Data.TryGetValue("ClientId", out var clientIdData))
                {
                    if (ulong.TryParse(clientIdData.Value, out ulong storedClientId) && storedClientId == clientId)
                    {
                        Debug.Log($"Found PlayerId: {player.Id} for Client ID: {clientId} on attempt {attempt}");
                        return player.Id;
                    }
                }
            }

            if (attempt < 3)
            {
                Debug.Log($"PlayerId for Client ID: {clientId} not found on attempt {attempt}, retrying after delay...");
                await Task.Delay(2000);
            }
        }

        Debug.LogWarning($"No PlayerId found for Client ID: {clientId} after all retries, attempting fallback...");
        string hostPlayerId = AuthenticationService.Instance.PlayerId;
        foreach (var player in joinedLobby.Players)
        {
            if (player.Id != hostPlayerId)
            {
                Debug.Log($"Fallback: Using PlayerId: {player.Id} for Client ID: {clientId}");
                return player.Id;
            }
        }

        Debug.LogWarning($"Fallback failed: No suitable PlayerId found for Client ID: {clientId}");
        return null;
    }

    private async void SendHeartbeat()
    {
        if (joinedLobby != null && IsHost)
        {
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    if (!AuthenticationService.Instance.IsSignedIn || !AuthenticationService.Instance.SessionTokenExists)
                    {
                        Debug.Log($"Authentication invalid before heartbeat for Profile: {profileId}. Re-authenticating...");
                        await SignInAnonymouslyWithRetry();
                    }
                    await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                    Debug.Log($"Heartbeat sent for lobby: {joinedLobby.Name}, ID: {joinedLobby.Id}, IsPrivate: {joinedLobby.IsPrivate}, Profile: {profileId}");
                    await VerifyLobbyStatus();
                    break;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Heartbeat attempt {attempt} failed for lobby {joinedLobby.Name}: {e.Message}");
                    if (attempt == 3)
                    {
                        Debug.LogError($"Failed to send heartbeat after 3 attempts: {e.Message}");
                        await RecreateLobbyIfNeeded();
                    }
                    await Task.Delay(2000);
                }
            }
        }
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            profileId = $"default_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            InitializationOptions initOptions = new InitializationOptions();
            initOptions.SetProfile(profileId);
            try
            {
                Debug.Log($"UnityServices.State before init: {UnityServices.State}, Profile: {profileId}");
                await UnityServices.InitializeAsync(initOptions);
                await SignInAnonymouslyWithRetry();
                Debug.Log($"Signed in anonymously with profile: {profileId}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services with profile {profileId}: {e.Message}");
            }
        }
    }

    private async Task SignInAnonymouslyWithRetry(int maxRetries = 3)
    {
        if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.SessionTokenExists)
        {
            Debug.Log($"Player already signed in with PlayerId: {AuthenticationService.Instance.PlayerId}, Profile: {profileId}");
            return;
        }
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Authentication successful on attempt {attempt}. PlayerId: {AuthenticationService.Instance.PlayerId}, Profile: {profileId}");
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Authentication attempt {attempt} failed: {e.Message}");
                if (attempt == maxRetries)
                {
                    Debug.LogError($"Failed to authenticate after {maxRetries} attempts: {e.Message}");
                    throw;
                }
                await Task.Delay(2000);
            }
        }
    }

    private async void MonitorRelayAllocation()
    {
        await Task.Delay(30 * 60 * 1000);
        if (joinedLobby != null && IsHost)
        {
            try
            {
                Debug.Log($"Refreshing Relay allocation for lobby: {joinedLobby.Name}");
                Allocation newAllocation = await RelayService.Instance.CreateAllocationAsync(max_user_amount);
                string newJoinCode = await RelayService.Instance.GetJoinCodeAsync(newAllocation.AllocationId);
                var updateOptions = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, newJoinCode) },
                        { "Password", new DataObject(DataObject.VisibilityOptions.Public, joinedLobby.Data["Password"].Value) }
                    }
                };
                joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, updateOptions);
                Debug.Log($"Relay allocation refreshed: New JoinCode={newJoinCode}");
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetHostRelayData(
                    newAllocation.RelayServer.IpV4,
                    (ushort)newAllocation.RelayServer.Port,
                    newAllocation.AllocationIdBytes,
                    newAllocation.Key,
                    newAllocation.ConnectionData
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to refresh Relay allocation: {e.Message}");
            }
        }
    }

    public async void CreateLobby(string lobbyName, string password, bool isPrivate)
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                Debug.Log("Initializing Unity Services...");
                profileId = $"default_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
                InitializationOptions initOptions = new InitializationOptions();
                initOptions.SetProfile(profileId);
                await UnityServices.InitializeAsync(initOptions);
                await SignInAnonymouslyWithRetry();
            }

            if (!AuthenticationService.Instance.IsSignedIn || !AuthenticationService.Instance.SessionTokenExists)
            {
                Debug.Log("Not signed in or invalid session, attempting to sign in...");
                await SignInAnonymouslyWithRetry();
            }

            Debug.Log($"Unity Services ready. Proceeding to Create Lobby with Profile: {profileId}");

            lobbyName = lobbyName.Trim().ToLower();
            password = password.Trim();

            QueryResponse existingLobbies = await Lobbies.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.Name,
                        value: lobbyName,
                        op: QueryFilter.OpOptions.EQ
                    )
                }
            });

            if (existingLobbies.Results.Count > 0)
            {
                Debug.LogError($"Lobby with name {lobbyName} already exists! Aborting creation.");
                if (LobbyUI.Instance != null)
                {
                    LobbyUI.Instance.UpdateStatus($"Error: Lobby '{lobbyName}' already exists!");
                }
                return;
            }

            Debug.Log($"Creating lobby: Name={lobbyName}, Password={password}, IsPrivate={isPrivate}");

            Debug.Log("Creating Relay allocation...");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(max_user_amount);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Relay allocation created: JoinCode={joinCode}");

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
                    { "Password", new DataObject(DataObject.VisibilityOptions.Public, password) }
                }
            };

            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, max_user_amount, options);
            Debug.Log($"Lobby created: {joinedLobby.Name}, ID: {joinedLobby.Id}");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport not found!");
                return;
            }

            transport.ConnectTimeoutMS = 15000;
            transport.DisconnectTimeoutMS = 30000;
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            Debug.Log("Starting host...");
            bool hostStarted = NetworkManager.Singleton.StartHost();
            Debug.Log($"Host started: {hostStarted}, IsHost: {NetworkManager.Singleton.IsHost}");

            InvokeRepeating(nameof(SendHeartbeat), 10f, 10f);
            KeepLobbyAlive();

            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.UpdateStatus("Host Started");
                LobbyUI.Instance.Hide();
            }

            MonitorRelayAllocation();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.UpdateStatus($"Error: {e.Message}");
            }
        }
    }

    private async void KeepLobbyAlive()
    {
        while (joinedLobby != null && IsHost)
        {
            try
            {
                var updateOptions = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "Heartbeat", new DataObject(DataObject.VisibilityOptions.Public, System.DateTime.UtcNow.ToString()) }
                    }
                };
                await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, updateOptions);
                Debug.Log("Sent lobby keep-alive update.");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to send keep-alive update: {e.Message}");
            }
            await Task.Delay(15000);
        }
    }

    /*public async void JoinLobbyByNameAndPassword(string lobbyName, string password)
    {
        // Reset error flag at the start of each join attempt
        hasReportedError = false;

        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.Log("UnityServices not initialized, initializing now...");
                profileId = $"default_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
                InitializationOptions initOptions = new InitializationOptions();
                initOptions.SetProfile(profileId);
                await UnityServices.InitializeAsync(initOptions);
                await SignInAnonymouslyWithRetry();
                Debug.Log($"UnityServices initialized and authenticated with Profile: {profileId}");
            }
            if (!AuthenticationService.Instance.IsSignedIn || !AuthenticationService.Instance.SessionTokenExists)
            {
                Debug.Log("Not signed in or invalid session, attempting to sign in...");
                await SignInAnonymouslyWithRetry();
            }
            Debug.Log($"Authentication status: IsSignedIn={AuthenticationService.Instance.IsSignedIn}, SessionTokenExists={AuthenticationService.Instance.SessionTokenExists}, PlayerId={AuthenticationService.Instance.PlayerId}, Profile: {profileId}");
            lobbyName = lobbyName.Trim().ToLower();
            Debug.Log($"Joining lobby: Name={lobbyName}, Password={password}");
            QueryResponse queryResponse = null;
            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    queryResponse = await Lobbies.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
                    {
                        Filters = new List<QueryFilter>
                        {
                            new QueryFilter(
                                field: QueryFilter.FieldOptions.Name,
                                value: lobbyName,
                                op: QueryFilter.OpOptions.EQ
                            )
                        }
                    });
                    Debug.Log($"Lobby query successful on attempt {attempt}: Found {queryResponse.Results.Count} lobbies.");
                    break;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Lobby query attempt {attempt} failed: {e.Message}");
                    if (attempt == maxRetries)
                    {
                        Debug.LogError($"Failed to query lobbies after {maxRetries} attempts: {e.Message}");
                        if (LobbyUI.Instance != null && !hasReportedError)
                        {
                            hasReportedError = true;
                            LobbyUI.Instance.UpdateStatus($"Error: Failed to query lobbies for {lobbyName}: {e.Message}");
                        }
                        return; // Exit after setting the error status
                    }
                    await Task.Delay(3000);
                }
            }
            if (queryResponse == null || queryResponse.Results.Count == 0)
            {
                Debug.LogError($"No lobbies found with name: {lobbyName}");
                if (LobbyUI.Instance != null && !hasReportedError)
                {
                    hasReportedError = true;
                    LobbyUI.Instance.UpdateStatus($"Error: No lobby named {lobbyName}");
                }
                return;
            }
            Lobby lobby = queryResponse.Results[0];

            // Perform password check before joining
            if (lobby.IsPrivate)
            {
                if (!lobby.Data.TryGetValue("Password", out var passwordData) || (passwordData.Value != password))
                {
                    Debug.LogError($"Incorrect password for private lobby {lobbyName}. Provided: {password}, Expected: {passwordData.Value}");
                    if (LobbyUI.Instance != null && !hasReportedError)
                    {
                        hasReportedError = true;
                        LobbyUI.Instance.UpdateStatus($"Error: Incorrect password for lobby {lobbyName}");
                    }
                    return; // Stop the join process if password is incorrect or missing
                }
                Debug.Log($"Password verified for private lobby {lobbyName}");
            }

            JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerId) }
                    }
                }
            };
            joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, joinOptions);
            Debug.Log($"Successfully joined lobby: {joinedLobby.Name}, ID: {joinedLobby.Id}, IsPrivate: {joinedLobby.IsPrivate}");

            if (lobby.Data.TryGetValue("JoinCode", out var joinCodeData))
            {
                string joinCode = joinCodeData.Value;
                Debug.Log($"Joining Relay with JoinCode={joinCode}");
                JoinAllocation joinAllocation = null;
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                        Debug.Log($"Relay join successful on attempt {attempt}.");
                        break;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Relay join attempt {attempt} failed: {e.Message}");
                        if (attempt == maxRetries)
                        {
                            Debug.LogError($"Failed to join Relay allocation after {maxRetries} attempts: {e.Message}");
                            if (LobbyUI.Instance != null && !hasReportedError)
                            {
                                hasReportedError = true;
                                LobbyUI.Instance.UpdateStatus($"Error: Failed to join Relay for {lobbyName}: {e.Message}");
                            }
                            return;
                        }
                        await Task.Delay(2000);
                    }
                }
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (transport == null)
                {
                    Debug.LogError("UnityTransport not found on NetworkManager!");
                    if (LobbyUI.Instance != null && !hasReportedError)
                    {
                        hasReportedError = true;
                        LobbyUI.Instance.UpdateStatus($"Error: UnityTransport not found for {lobbyName}");
                    }
                    return;
                }
                transport.ConnectTimeoutMS = 15000;
                transport.DisconnectTimeoutMS = 30000;
                transport.SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );
                try
                {
                    bool clientStarted = NetworkManager.Singleton.StartClient();
                    Debug.Log($"Client started: {clientStarted}, IsClient: {NetworkManager.Singleton.IsClient}");
                    if (clientStarted)
                    {
                        await Task.Delay(2000);
                        string clientId = NetworkManager.Singleton.LocalClientId.ToString();
                        var updatePlayerOptions = new UpdatePlayerOptions
                        {
                            Data = new Dictionary<string, PlayerDataObject>
                            {
                                { "ClientId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, clientId) }
                            }
                        };
                        for (int attempt = 1; attempt <= 5; attempt++)
                        {
                            try
                            {
                                joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
                                Debug.Log($"Updated player data with ClientId: {clientId} for PlayerId: {AuthenticationService.Instance.PlayerId} on attempt {attempt}");
                                break;
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogWarning($"UpdatePlayerAsync attempt {attempt} failed: {e.Message}");
                                if (attempt == 5)
                                {
                                    Debug.LogError($"Failed to update player data after 5 attempts: {e.Message}");
                                    break;
                                }
                                await Task.Delay(2000);
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to start client: {e.Message}");
                    if (LobbyUI.Instance != null && !hasReportedError)
                    {
                        hasReportedError = true;
                        LobbyUI.Instance.UpdateStatus($"Error: Failed to start client for {lobbyName}: {e.Message}");
                    }
                    return;
                }
                if (LobbyUI.Instance != null)
                {
                    LobbyUI.Instance.UpdateStatus("Client Connected");
                    LobbyUI.Instance.Hide();
                }
                else
                {
                    Debug.LogError("LobbyUI instance not found!");
                }
            }
            else
            {
                Debug.LogError("No join code found in lobby data.");
                if (LobbyUI.Instance != null && !hasReportedError)
                {
                    hasReportedError = true;
                    LobbyUI.Instance.UpdateStatus($"Error: No join code found for {lobbyName}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
            if (LobbyUI.Instance != null && !hasReportedError)
            {
                hasReportedError = true;
                LobbyUI.Instance.UpdateStatus($"Error: Failed to join lobby {lobbyName}: {e.Message}");
            }
        }
    }*/

    public async void JoinLobbyByNameAndPassword(string lobbyName, string password)
    {
        try
        {
            await EnsureUnityServicesInitializedAsync();

            // Ensure signed in
            if (!AuthenticationService.Instance.IsSignedIn || !AuthenticationService.Instance.SessionTokenExists)
            {
                Debug.Log("Signing in anonymously...");
                await SignInAnonymouslyWithRetry();
            }

            lobbyName = lobbyName.Trim().ToLower();
            Debug.Log($"Searching for lobby with name: {lobbyName}");

            var lobby = await FindLobbyByNameAsync(lobbyName);
            if (lobby == null)
            {
                Debug.LogError($"Lobby not found: {lobbyName}");
                LobbyUI.Instance?.UpdateStatus($"Error: No lobby named {lobbyName}");
                return;
            }

            //  Validate password before joining
            if (lobby.Data.TryGetValue("Password", out var passwordData))
            {
                if (passwordData.Value != password)
                {
                    Debug.LogError("Incorrect password.");
                    LobbyUI.Instance?.UpdateStatus("Error: Incorrect password!");
                    return;
                }
            }

            //  Only join if password is correct
            joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            Debug.Log($"Joined lobby: {joinedLobby.Name}");

            // Proceed to join Relay if join code exists
            if (lobby.Data.TryGetValue("JoinCode", out var joinCodeData))
            {
                await JoinRelayAsync(joinCodeData.Value);
            }
            else
            {
                Debug.LogError("No JoinCode in lobby.");
                LobbyUI.Instance?.UpdateStatus("Error: No join code found!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Join lobby failed: {e.Message}");
            LobbyUI.Instance?.UpdateStatus($"Error: {e.Message}");
        }
    }

    private async Task EnsureUnityServicesInitializedAsync()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized) return;

        profileId = $"default_{Guid.NewGuid().ToString().Substring(0, 8)}";
        InitializationOptions initOptions = new InitializationOptions().SetProfile(profileId);
        await UnityServices.InitializeAsync(initOptions);
        await SignInAnonymouslyWithRetry();

        Debug.Log($"Unity Services initialized with profile: {profileId}");
    }

    private async Task<Lobby> FindLobbyByNameAsync(string name)
    {
        const int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var query = new QueryLobbiesOptions
                {
                    Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.Name, name, QueryFilter.OpOptions.EQ)
                }
                };

                var result = await Lobbies.Instance.QueryLobbiesAsync(query);
                if (result.Results.Count > 0)
                {
                    Debug.Log($"Lobby found: {result.Results[0].Name}");
                    return result.Results[0];
                }

                Debug.LogWarning($"No lobby found on attempt {attempt}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Lobby query failed on attempt {attempt}: {ex.Message}");
            }

            await Task.Delay(2000);
        }

        return null;
    }

    private async Task JoinRelayAsync(string joinCode)
    {
        const int maxRetries = 3;
        JoinAllocation allocation = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                Debug.Log("Successfully joined Relay.");
                break;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Relay join attempt {attempt} failed: {e.Message}");
                if (attempt == maxRetries)
                {
                    LobbyUI.Instance?.UpdateStatus($"Error: Relay join failed.");
                    return;
                }

                await Task.Delay(1500);
            }
        }

        if (allocation == null)
        {
            Debug.LogError("Relay allocation is null.");
            return;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("UnityTransport not found on NetworkManager.");
            return;
        }

        transport.SetClientRelayData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            allocation.HostConnectionData
        );

        try
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started successfully.");
            LobbyUI.Instance?.UpdateStatus("Client Connected");
            LobbyUI.Instance?.Hide();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Client failed to start: {ex.Message}");
            LobbyUI.Instance?.UpdateStatus($"Error: {ex.Message}");
        }
    }



    private async Task VerifyLobbyStatus()
    {
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn || !AuthenticationService.Instance.SessionTokenExists)
            {
                Debug.Log($"Authentication invalid before verifying lobby status. Re-authenticating...");
                await SignInAnonymouslyWithRetry();
            }
            var lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            Debug.Log($"Lobby status verified: {lobby.Name} is active with {lobby.Players.Count} players, IsPrivate: {lobby.IsPrivate}, Profile: {profileId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to verify lobby status: {e.Message}");
            await RecreateLobbyIfNeeded();
        }
    }

    private async Task RecreateLobbyIfNeeded()
    {
        try
        {
            await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lobby {joinedLobby.Name} no longer exists: {e.Message}");
            string lobbyName = joinedLobby.Name;
            string password = joinedLobby.Data["Password"].Value;
            bool isPrivate = joinedLobby.IsPrivate;
            joinedLobby = null;
            CreateLobby(lobbyName, password, isPrivate);
        }
    }

    public Unity.Services.Lobbies.Models.Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }
}