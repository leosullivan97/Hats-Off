using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraManager : NetworkBehaviour
{
    [Header("Elements")]
    [SerializeField] private CinemachineTargetGroup targetGroup;

    private bool configured;
    private List<PlayerController> playerControllers = new List<PlayerController>();

    private void Start()
    {
        NetworkManager.OnServerStarted += ServerStartedCallBack;
    }

    private void ServerStartedCallBack()
    {
        if (!IsServer) return;

        NetworkManager.OnClientConnectedCallback += ClientConnectedCallback;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (!IsServer) return;

        NetworkManager.OnClientConnectedCallback -= ClientConnectedCallback;
    }

    private void ClientConnectedCallback(ulong clientId)
    {
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;

        if (playerCount < 2) return; // Only configure the camera when at least 2 players are connected

        StorePlayersRpc();
        UpdateCameraTargetGroupRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void StorePlayersRpc()
    {
        // Get all active PlayerController instances
        PlayerController[] playerControllersArray = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        playerControllers = new List<PlayerController>(playerControllersArray);
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateCameraTargetGroupRpc()
    {
        configured = true;

        foreach (PlayerController playerController in playerControllers)
        {
            float weight = (playerController.OwnerClientId == NetworkManager.Singleton.LocalClientId) ? 10 : 1;

            targetGroup.AddMember(playerController.transform, weight, 2);
        }
    }
}
