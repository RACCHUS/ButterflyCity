using UnityEngine;
using Unity.Netcode;

// Lightweight network bridge that lets the server broadcast the connected player list to clients
public class PlayerListNetwork : NetworkBehaviour
{
    private RelayLobbyManager owner;

    public void Init(RelayLobbyManager mgr)
    {
        owner = mgr;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            // Subscribe to client connect events
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // When someone connects, broadcast the current list to all clients
        BroadcastPlayerList();
    }

    public void BroadcastPlayerList()
    {
        if (!IsServer || owner == null) return;
        var names = owner.GetConnectedPlayerNames();
        string payload = string.Join("|", names);
        SendPlayerListClientRpc(payload);
    }

    [ClientRpc]
    private void SendPlayerListClientRpc(string payload)
    {
        // Clients (and host) receive the current player list and update their UI
        // Use the newer API to avoid obsolete FindObjectOfType warnings
        var ui = UnityEngine.Object.FindFirstObjectByType<RelayLobbyUI_PlayerList>();
        if (ui == null) return;
        string[] names = string.IsNullOrEmpty(payload) ? new string[0] : payload.Split('|');
        ui.UpdatePlayerList(names);
    }

    // Explicitly hide the base implementation (NetworkBehaviour/MonoBehaviour) to
    // acknowledge we intentionally provide a new OnDestroy here.
    private new void OnDestroy()
    {
        if (NetworkManager != null)
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
    }
}
