using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimator : NetworkBehaviour
{
    [Header("Components")]
    private PlayerController playerController; // Reference to PlayerController for state changes

    [Header("Elements")]
    [SerializeField] private Animator animator; // Animator to control animations

    // Subscribe to player movement state events
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerController.onJumpStart += Jump;
        playerController.onFallStart += Fall;
        playerController.onLand += Land;
    }

    // Unsubscribe from events when destroyed
    private void OnDestroy()
    {
        playerController.onJumpStart -= Jump;
        playerController.onFallStart -= Fall;
        playerController.onLand -= Land;
    }

    void Start() { }

    // Update blend tree parameter every frame
    void Update()
    {
        UpdateBlendTreeRpc();
    }

    // Sync xVelocity across the network for all clients
    [Rpc(SendTo.Everyone)]
    private void UpdateBlendTreeRpc()
    {
        animator.SetFloat("xVelocity", playerController.XVelocity);
    }

    // Trigger jump animation
    private void Jump()
    {
        animator.Play("Jump");
    }

    // Trigger fall animation
    private void Fall()
    {
        AudioManager.instance.PlayFallSound();
        animator.Play("Fall");
    }

    // Trigger land animation
    private void Land()
    {
        AudioManager.instance.PlayLandSound();
        animator.Play("Land");
    }
}
