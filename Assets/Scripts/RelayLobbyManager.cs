using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayLobbyManager : MonoBehaviour
{
    private Lobby hostLobby;
    private const int MaxPlayers = 4; // Adjust for your game

    async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("✅ Signed in: " + AuthenticationService.Instance.PlayerId);
        }
    }

    // Host creates Relay allocation + Lobby
    public async Task<string> HostGame()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            hostLobby = await LobbyService.Instance.CreateLobbyAsync("BattleCityRoom", MaxPlayers, new CreateLobbyOptions
            {
                IsPrivate = true,
                Data = { { "JoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) } }
            });

            Debug.Log("✅ Hosting Lobby with Join Code: " + joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(allocation.RelayServer.IpV4,
                                         (ushort)allocation.RelayServer.Port,
                                         allocation.AllocationIdBytes,
                                         allocation.Key,
                                         allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ HostGame failed: " + e);
            return null;
        }
    }

    // Client joins Relay + Lobby with room code
    public async Task JoinGame(string joinCode)
    {
        try
        {
            // Connect to Relay
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(joinAllocation.RelayServer.IpV4,
                                         (ushort)joinAllocation.RelayServer.Port,
                                         joinAllocation.AllocationIdBytes,
                                         joinAllocation.Key,
                                         joinAllocation.ConnectionData,
                                         joinAllocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
            Debug.Log("✅ Joined Relay with Join Code: " + joinCode);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ JoinGame failed: " + e);
        }
    }
}