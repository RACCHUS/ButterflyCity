
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using System.Collections.Generic;


public class RelayLobbyManager : MonoBehaviour
{
    [Header("References")]
    public NetworkManager netManager; // Drag your DontDestroyOnLoad NetworkManager here, or it will auto-find
    private UnityTransport transport;
    private Lobby hostLobby;
    private const int MaxPlayers = 4;
    private bool servicesInitialized = false;

        private async void Start()
        {
            // Auto-find NetworkManager if not set
            if (netManager == null)
            {
                netManager = FindFirstObjectByType<NetworkManager>();
                if (netManager == null)
                {
                    Debug.LogError("NetworkManager reference not set in RelayLobbyManager!");
                    return;
                }
            }
            transport = netManager.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport component missing on NetworkManager!");
                return;
            }
            // Ensure PlayerListNetwork bridge exists on the NetworkManager so server can broadcast lists
            var plNet = netManager.gameObject.GetComponent<PlayerListNetwork>() ?? netManager.gameObject.AddComponent<PlayerListNetwork>();
            plNet.Init(this);
            // Initialize Unity Services
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await Task.Yield();
                servicesInitialized = true;
                Debug.Log($"✅ Unity Services Initialized & Signed In: {AuthenticationService.Instance.PlayerId}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to initialize Unity Services: {e}");
                servicesInitialized = false;
            }
        }

        public bool AreServicesReady()
        {
            return servicesInitialized
                && UnityServices.State == ServicesInitializationState.Initialized
                && AuthenticationService.Instance.IsSignedIn
                && RelayService.Instance != null
                && LobbyService.Instance != null;
        }

        public async Task<string> HostGame()
        {
            if (netManager == null || transport == null)
            {
                Debug.LogError("NetworkManager or Transport is null!");
                return null;
            }
            if (!AreServicesReady())
            {
                Debug.LogError("❌ Unity Services not ready! Make sure services are initialized and user is signed in.");
                return null;
            }
            try
            {
                // Create Relay allocation
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                // Detailed allocation info for debugging
                Debug.Log($"✅ Relay Allocation OK. JoinCode: {joinCode} | AllocationId: {allocation.AllocationId} | Relay IP: {allocation.RelayServer.IpV4}:{allocation.RelayServer.Port}");
                Debug.Log($"Allocation details: KeyLength={allocation.Key?.Length ?? 0}, ConnDataLength={allocation.ConnectionData?.Length ?? 0}");

                // Configure UnityTransport for Relay
                transport.SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                // Start host
                netManager.StartHost();
                Debug.Log("✅ Host started successfully (netManager.StartHost invoked). Waiting for network state...");
                // small delay to let Netcode update state, then log status
                await Task.Delay(200);
                Debug.Log($"NetManager status after StartHost: IsListening={netManager.IsListening}, IsServer={netManager.IsServer}, IsHost={netManager.IsHost}");
                // Broadcast initial player list to clients (host should also update its UI)
                var plBridge = netManager.gameObject.GetComponent<PlayerListNetwork>();
                if (plBridge != null)
                    plBridge.BroadcastPlayerList();

                // Optionally create a Lobby
                    try
                    {
                        var lobbyOptions = new CreateLobbyOptions { IsPrivate = true };
                        hostLobby = await LobbyService.Instance.CreateLobbyAsync("BattleCityRoom", MaxPlayers, lobbyOptions);
                        Debug.Log($"✅ Lobby created. ID: {hostLobby.Id}");
                    }
                    catch (System.Exception lobbyEx)
                    {
                        Debug.LogWarning($"Failed to create lobby: {lobbyEx}");
                    }
                return joinCode;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ HostGame failed: {e}\n{e.StackTrace}");
                return null;
            }
        }

        public async Task JoinGame(string joinCode)
        {
            if (netManager == null || transport == null)
            {
                Debug.LogError("NetworkManager or Transport is null!");
                return;
            }
            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogWarning("Join code is empty!");
                return;
            }
            if (!AreServicesReady())
            {
                Debug.LogError("❌ Unity Services not ready for joining game!");
                return;
            }
            try
            {
                // Join Relay allocation
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                Debug.Log($"✅ JoinAllocation received. AllocationId: {joinAllocation.AllocationId} | HostConnDataLength={(joinAllocation.HostConnectionData?.Length ?? 0)} | Relay IP: {joinAllocation.RelayServer.IpV4}:{joinAllocation.RelayServer.Port}");
                // Configure UnityTransport for Relay client
                transport.SetRelayServerData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );
                Debug.Log("Transport configured for Relay join (SetRelayServerData invoked).");
                // Start client
                netManager.StartClient();
                Debug.Log("netManager.StartClient invoked, waiting for connection state...");
                // wait up to 5 seconds for the client to connect
                int wait = 0;
                while (wait < 50 && !(netManager.IsConnectedClient))
                {
                    await Task.Delay(100);
                    wait++;
                }
                Debug.Log($"NetManager status after StartClient: IsClient={netManager.IsClient}, IsConnectedClient={netManager.IsConnectedClient}, IsListening={netManager.IsListening}");
                if (netManager.IsConnectedClient)
                {
                    Debug.Log($"✅ Joined game with code: {joinCode}");
                }
                else
                {
                    Debug.LogWarning($"Join attempt did not reach connected state within timeout. IsConnectedClient={netManager.IsConnectedClient}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ JoinGame failed: {e}\n{e.StackTrace}");
            }
        }

        public void LeaveGame()
        {
            if (netManager != null && netManager.IsListening)
            {
                netManager.Shutdown();
                Debug.Log("NetworkManager shut down.");
            }
            else
            {
                Debug.Log("LeaveGame called, but NetworkManager was not running.");
            }
            // Optionally: clean up lobby/relay allocations if needed
            hostLobby = null;
        }

        // Return a simple list of connected player names for UI display.
        // Shows each connected client as "Player <id>" and marks the host next to its player entry
        // (e.g. "Player 0 (Host)"). This avoids a separate "Host" entry that duplicates the host.
        public string[] GetConnectedPlayerNames()
        {
            var names = new System.Collections.Generic.List<string>();
            if (netManager == null) return names.ToArray();
            try
            {
                // ConnectedClientsList contains NetworkClient objects with ClientId
                var list = netManager.ConnectedClientsList;
                if (list != null)
                {
                    // Determine the server/host client id if available; fall back to 0
                    ulong hostClientId = 0UL;
                    try { hostClientId = NetworkManager.ServerClientId; } catch { hostClientId = 0UL; }

                    foreach (var c in list)
                    {
                        try
                        {
                            string label = $"Player {c.ClientId}";
                            if (c.ClientId == hostClientId)
                                label += " (Host)";
                            names.Add(label);
                        }
                        catch
                        {
                            names.Add("Player");
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                // if any reflection/API mismatch happens, return minimal info
            }
            return names.ToArray();
        }
    }

