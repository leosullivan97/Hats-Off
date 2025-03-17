using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerPlacer : NetworkBehaviour
{
    [Header("Elements")]
    [SerializeField] private Transform[] spawnPositions;
    private List<Transform> potentialPositions;

    private void Start()
    {
        // Store spawn positions for random selection
        potentialPositions = new List<Transform>(spawnPositions);
        NetworkManager.OnClientConnectedCallback += ClientConnectedCallback;
    }

    public override void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        NetworkManager.OnClientConnectedCallback -= ClientConnectedCallback;
    }

    private void ClientConnectedCallback(ulong clientId)
    {
        if (!IsServer) return;

        // Select a random available spawn position
        int positionIndex = Random.Range(0, potentialPositions.Count);
        Vector3 spawnPosition = potentialPositions[positionIndex].position;
        potentialPositions.RemoveAt(positionIndex);

        // Assign the player to the chosen position
        PlacePlayerRpc(spawnPosition, clientId);
    }

    [Rpc(SendTo.Everyone)]
    private void PlacePlayerRpc(Vector3 spawnPosition, ulong clientId)
    {
        // Only move the local player
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        NetworkManager.Singleton.LocalClient.PlayerObject.transform.position = spawnPosition;
    }
}
