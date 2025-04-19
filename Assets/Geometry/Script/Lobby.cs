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

public class Lobby : MonoBehaviour
{
    public static Lobby Instance { get; private set; }

    public const int max_user_amount = 40;
    private Unity.Services.Lobbies.Models.Lobby joinedLobby;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No DontDestroyOnLoad
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeUnityAuthentication();
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(Random.Range(0, 10000).ToString());

            try
            {
                await UnityServices.InitializeAsync(initializationOptions);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Unity Services initialized and signed in anonymously.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
            }
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            // Create Relay allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(max_user_amount);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Store join code in lobby data
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, max_user_amount, options);
            Debug.Log($"Lobby created: {joinedLobby.Name} (ID: {joinedLobby.Id}), JoinCode: {joinCode}");

            // Initialize Netcode host with Relay
            if (NetworkManager.Singleton != null)
            {
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (transport != null)
                {
                    transport.SetHostRelayData(
                        allocation.RelayServer.IpV4,
                        (ushort)allocation.RelayServer.Port,
                        allocation.AllocationIdBytes,
                        allocation.Key,
                        allocation.ConnectionData
                    );
                    Debug.Log("Configured UnityTransport for Relay host.");
                }
                else
                {
                    Debug.LogError("UnityTransport component not found on NetworkManager!");
                    return;
                }

                NetworkManager.Singleton.StartHost();
                Debug.Log("Netcode host started with Relay.");

                // Hide LobbyUI
                if (LobbyUI.Instance != null)
                {
                    LobbyUI.Instance.Hide();
                }
                else
                {
                    Debug.LogError("LobbyUI instance not found! Cannot hide UI.");
                }
            }
            else
            {
                Debug.LogError("NetworkManager is not found! Ensure it’s in the scene.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create lobby or Relay allocation: {e.Message}");
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log($"Joined lobby: {joinedLobby.Name} (ID: {joinedLobby.Id})");

            // Initialize Netcode client with Relay
            if (NetworkManager.Singleton != null)
            {
                if (joinedLobby.Data != null && joinedLobby.Data.ContainsKey("JoinCode"))
                {
                    string joinCode = joinedLobby.Data["JoinCode"].Value;
                    JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                    var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                    if (transport != null)
                    {
                        transport.SetClientRelayData(
                            allocation.RelayServer.IpV4,
                            (ushort)allocation.RelayServer.Port,
                            allocation.AllocationIdBytes,
                            allocation.Key,
                            allocation.ConnectionData,
                            allocation.HostConnectionData
                        );
                        Debug.Log($"Client joined Relay with join code: {joinCode}");
                    }
                    else
                    {
                        Debug.LogError("UnityTransport component not found on NetworkManager!");
                        return;
                    }
                }
                else
                {
                    Debug.LogError("JoinCode not found in lobby data!");
                    return;
                }

                NetworkManager.Singleton.StartClient();
                Debug.Log("Netcode client started.");

               
                if (LobbyUI.Instance != null)
                {
                    LobbyUI.Instance.Hide();
                }
                else
                {
                    Debug.LogError("Lobby UI");
                }
            }
            else
            {
                Debug.LogError("NetworkManager is not found! Ensure it’s in the scene.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to quick join lobby or Relay allocation: {e.Message}");
        }
    }

    public Unity.Services.Lobbies.Models.Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }
}