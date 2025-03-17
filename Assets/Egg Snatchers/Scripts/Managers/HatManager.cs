using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HatManager : NetworkBehaviour
{
    [Header("Elements")]
    [SerializeField] private GameObject hatPrefab;
    [SerializeField] private Transform[] spawnPositions;

    private void Start()
    {
        NetworkManager.OnServerStarted += ServerStartedCallBack;
    }

    public override void OnDestroy()
    {
        NetworkManager.OnServerStarted -= ServerStartedCallBack;
    }

    private void ServerStartedCallBack()
    {
        if (!IsServer) return; // Only the server should spawn the hat

        SpawnHat();
    }

    private void SpawnHat()
    {
        // Choose a random spawn position
        Vector3 spawnPosition = spawnPositions[Random.Range(0, spawnPositions.Length)].position;

        // Instantiate the hat and spawn it on the network
        GameObject hatInstance = Instantiate(hatPrefab, spawnPosition, Quaternion.identity, transform);
        hatInstance.GetComponent<NetworkObject>().Spawn();
    }
}
