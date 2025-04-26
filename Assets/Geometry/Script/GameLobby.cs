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


public class GameLobby : MonoBehaviour
{
    public static GameLobby Instance { get; private set; }
    public const int max_user_amount = 40;
    private Unity.Services.Lobbies.Models.Lobby joinedLobby;

    private void Start()
    {
        // Start heartbeat for host
        if (IsHost)
        {
            InvokeRepeating(nameof(SendHeartbeat), 15f, 15f); // Every 15 seconds
        }
    }
    private bool IsHost
    {
        get
        {
            return NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        }
    }

    private async void SendHeartbeat()
    {
        if (joinedLobby != null && IsHost)
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                Debug.Log($"Heartbeat sent for lobby: {joinedLobby.Name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to send heartbeat: {e.Message}");
            }
        }
    }
    /*  SendHeartbeatPingAsync keeps the lobby alive.
    Called every 15 seconds(within the 30 second timeout).
    Only runs on the host(server).*/

    private void Awake()
    {
        Debug.Log("GameLobby Awake - This instance: " + this);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameLobby Instance set successfully.");
        }
        else
        {
            Debug.LogWarning("GameLobby already exists, destroying duplicate!");
            Destroy(gameObject);
        }
    }


    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(Random.Range(0, 10000).ToString());

            try
            {
                await UnityServices.InitializeAsync(options);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
            }
        }
    }

    // In GameLobby.cs, add to CreateLobby after lobby creation
    private async void MonitorRelayAllocation()
    {
        // Example: Refresh after 50 minutes
        await Task.Delay(50 * 60 * 1000); // 50 minutes
        if (joinedLobby != null && IsHost)
        {
            try
            {
                Debug.Log("Refreshing Relay allocation");
                Allocation newAllocation = await RelayService.Instance.CreateAllocationAsync(max_user_amount);
                string newJoinCode = await RelayService.Instance.GetJoinCodeAsync(newAllocation.AllocationId);

                // Update lobby with new join code
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

                // Update transport
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

    /*public async void CreateLobby(string lobbyName, string password, bool isPrivate)
    {
        try
        {
            lobbyName = lobbyName.Trim();
            password = password.Trim();
            Debug.Log($"Creating lobby: Name={lobbyName}, Password={password}");

            // Create Relay allocation
            Debug.Log("Creating Relay allocation");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(max_user_amount);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Relay allocation created: JoinCode={joinCode}");

            // Create lobby
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
            Debug.Log($"Lobby created: Name={joinedLobby.Name}, ID={joinedLobby.Id}, JoinCode={joinCode}, Password={password}");

            // Configure UnityTransport
            Debug.Log("Configuring UnityTransport");
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport component not found on NetworkManager!");
                return;
            }

            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            Debug.Log("UnityTransport configured successfully");

            // Start host
            Debug.Log("Starting host");
            try
            {
                NetworkManager.Singleton.StartHost();
                Debug.Log("Host started successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start host: {e.Message}");
                return;
            }

            // Hide UI
            Debug.Log("Hiding UI");
            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.UpdateStatus("Host Started");
                LobbyUI.Instance.Hide();
            }
            else
            {
                Debug.LogError("LobbyUI instance not found!");
            }

            // Start monitoring Relay (optional, for long sessions)
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
    }*/

    public async void CreateLobby(string lobbyName, string password, bool isPrivate)
    {
        try
        {
            //  Ensure UnityServices is initialized
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.Log("UnityServices not initialized, initializing now...");
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("UnityServices initialized and authenticated.");
            }

            
            lobbyName = lobbyName.Trim();
            password = password.Trim();
            Debug.Log($"Creating lobby: Name={lobbyName}, Password={password}");

            // Create Relay allocation
            Debug.Log("Creating Relay allocation");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(max_user_amount);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Relay allocation created: JoinCode={joinCode}");

            // Create lobby
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
            Debug.Log($"Lobby created: Name={joinedLobby.Name}, ID={joinedLobby.Id}, JoinCode={joinCode}, Password={password}");

            // Configure UnityTransport
            Debug.Log("Configuring UnityTransport");
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport component not found on NetworkManager!");
                return;
            }

            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            Debug.Log("UnityTransport configured successfully");

            // Start host
            Debug.Log("Starting host");
            try
            {
                NetworkManager.Singleton.StartHost();
                Debug.Log("Host started successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start host: {e.Message}");
                return;
            }

            // Hide UI
            Debug.Log("Hiding UI");
            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.UpdateStatus("Host Started");
                LobbyUI.Instance.Hide();
            }
            else
            {
                Debug.LogError("LobbyUI instance not found!");
            }

            // Start monitoring Relay (optional, for long sessions)
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




    public async void JoinLobbyByNameAndPassword(string lobbyName, string password)
    {
        try
        {
            lobbyName = lobbyName.Trim();
            password = password.Trim();
            Debug.Log($"Joining lobby: Name={lobbyName}, Password={password}");

            // Query lobbies
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 50,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.Name, lobbyName, QueryFilter.OpOptions.EQ)
                }
            };

            Debug.Log("Querying lobbies");
            QueryResponse response = null;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                response = await LobbyService.Instance.QueryLobbiesAsync(options);
                Debug.Log($"Query attempt {attempt}: Found {response.Results.Count} lobbies");
                if (response.Results.Count > 0) break;
                Debug.Log($"No lobbies found, retrying in 1s...");
                await Task.Delay(1000);
            }

            if (response == null || response.Results.Count == 0)
            {
                Debug.LogError($"No lobby found with name: {lobbyName}");
                if (LobbyUI.Instance != null)
                {
                    LobbyUI.Instance.UpdateStatus($"Error: No lobby named {lobbyName}");
                }
                return;
            }

            // Find lobby with matching password
            Unity.Services.Lobbies.Models.Lobby targetLobby = null;
            foreach (var lobby in response.Results)
            {
                Debug.Log($"Checking lobby: Name={lobby.Name}, ID={lobby.Id}, Data={(lobby.Data != null ? string.Join(", ", lobby.Data.Select(kvp => $"{kvp.Key}: {kvp.Value.Value}")) : "None")}");
                if (lobby.Data != null && lobby.Data.ContainsKey("Password") && lobby.Data["Password"].Value == password)
                {
                    targetLobby = lobby;
                    break;
                }
            }

            if (targetLobby == null)
            {
                Debug.LogError("Incorrect password or lobby not found!");
                if (LobbyUI.Instance != null)
                {
                    LobbyUI.Instance.UpdateStatus("Error: Incorrect password or lobby name!");
                }
                return;
            }

            // Join lobby
            Debug.Log($"Joining lobby: {targetLobby.Name}");
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(targetLobby.Id);
            Debug.Log($"Joined lobby: {joinedLobby.Name}");

            // Configure Relay for client
            Debug.Log("Configuring Relay for client");
            if (targetLobby.Data != null && targetLobby.Data.ContainsKey("JoinCode"))
            {
                string joinCode = targetLobby.Data["JoinCode"].Value;
                Debug.Log($"Fetching JoinAllocation with JoinCode: {joinCode}");
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (transport == null)
                {
                    Debug.LogError("UnityTransport component not found on NetworkManager!");
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
                Debug.Log("UnityTransport configured for client");
            }
            else
            {
                Debug.LogError("JoinCode not found in lobby data!");
                return;
            }

            // Start client
            Debug.Log("Starting client");
            try
            {
                NetworkManager.Singleton.StartClient();
                Debug.Log("Client started successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start client: {e.Message}");
                return;
            }

            // Hide UI
            /*Debug.Log("Hiding UI");
            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.UpdateStatus("Client Joined");
                LobbyUI.Instance.Hide();
            }
            else
            {
                Debug.LogError("LobbyUI instance not found!");
            }*/
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.UpdateStatus($"Error: {e.Message}");
            }
        }
    }

    public Unity.Services.Lobbies.Models.Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }
}