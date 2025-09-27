using Unity.Netcode;
using UnityEngine;

public class TankSpawner : NetworkBehaviour
{
    public GameObject tankPrefab;           // Assign your Tank prefab here
    public Transform[] spawnPoints;         // Assign spawn points in Inspector

    public override void OnNetworkSpawn()
    {
        if (IsServer)  // Only the host spawns tanks
        {
            SpawnTanks();
        }
    }

    void SpawnTanks()
    {
        // Example: spawn one tank per connected player
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // Pick a spawn point (simple round-robin)
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Instantiate tank prefab
            GameObject tank = Instantiate(tankPrefab, spawn.position, Quaternion.identity);

            // Spawn over network and assign ownership to client
            tank.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        }
    }
}