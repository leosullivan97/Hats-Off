using System.Collections;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(PlayerController))]
public class PlayerDetection : NetworkBehaviour
{
    [Header("Components")]
    private PlayerController playerController;

    [Header("Elements")]
    [SerializeField] private LayerMask groundMask; // Layer for ground detection
    [SerializeField] private LayerMask trampolineMask; // Layer for trampolines
    [SerializeField] private LayerMask hatMask; // Layer for hats
    [SerializeField] private BoxCollider boxCollider; // Collider for ground detection
    [SerializeField] private CapsuleCollider capsuleCollider; // Player's main collider

    [Header("Settings")]
    private bool canStealHat; // Prevents immediate re-stealing after losing a hat
    public bool IsHoldingHat { get; private set; } // Tracks if the player has a hat

    private void Start()
    {
        canStealHat = true;
        playerController = GetComponent<PlayerController>(); // Get reference to PlayerController
    }

    private void Update()
    {
        DetectTrampolines(); // Check for trampolines each frame

        if (IsServer) 
        {
            DetectHats(); // Only the server checks for hats
        }
    }

    private void DetectTrampolines()
    {
        // Only check if the player is on the ground
        if (!IsGrounded()) return;

        // If a trampoline is detected below the player, trigger a jump
        if (Physics.OverlapBox(boxCollider.transform.position, boxCollider.size / 2, Quaternion.identity, trampolineMask).Length > 0)
        {
            AudioManager.instance.PlayTrampolineSound();
            playerController.Jump();
        }
    }

    private void DetectHats()
    {
        // Don't detect hats if the player already has one or can't steal
        if (!canStealHat || IsHoldingHat) return;

        Collider[] detectedHats = DetectColliders(transform.position, hatMask, out Collider hat);

        if (hat == null) return; // No hats detected, exit

        // If the hat is not attached to another player, pick it up
        if (hat.transform.parent == null)
        {
            GrabHat(hat);
        }
        else if (hat.transform.parent.TryGetComponent(out PlayerDetection playerDetection))
        {
            // If another player has the hat, steal it
            StealHatFrom(playerDetection, hat);
        }

        GrabHat(hat); // Ensure the player grabs the hat
        Debug.Log("Hat detected.");
    }

    private void StealHatFrom(PlayerDetection otherPlayer, Collider hat)
    {
        GrabHat(hat); // Take the hat
        otherPlayer.LoseHat(); // Make the other player lose the hat
    }

    private void LoseHat()
    {
        IsHoldingHat = false; // Player no longer holds a hat
        canStealHat = false; // Prevents immediate hat stealing

        StartCoroutine(LoseHatCooldown()); // Start cooldown before the player can steal again
    }

    private IEnumerator LoseHatCooldown()
    {
        AudioManager.instance.PlayLoseHatSound();
        yield return new WaitForSecondsRealtime(1); // Wait 1 second before allowing hat stealing again
        canStealHat = true;
    }

    private void GrabHat(Collider hat)
    {
        // Attach the hat to the player
        AudioManager.instance.PlayGrabHatSound();
        hat.transform.SetParent(transform);
        hat.transform.localPosition = Vector3.up * 3.1f; // Position the hat above the player's head
        IsHoldingHat = true;
    }

    public bool CanGoThere(Vector3 targetPosition, out Collider firstCollider)
    {
        // Check if there's ground at the target position
        Collider[] detectedColliders = DetectColliders(targetPosition, groundMask, out firstCollider);
        return detectedColliders.Length == 0; // If no ground is detected, movement is allowed
    }

    private Collider[] DetectColliders(Vector3 position, LayerMask mask, out Collider firstCollider)
    {
        // Define the capsule collider's top and bottom points
        Vector3 center = position + capsuleCollider.center;
        float halfHeight = (capsuleCollider.height / 2) - capsuleCollider.radius;
        Vector3 offset = transform.up * halfHeight;
        Vector3 point0 = center + offset; // Top of the capsule
        Vector3 point1 = center - offset; // Bottom of the capsule

        // Detect colliders within the capsule
        Collider[] colliders = Physics.OverlapCapsule(point0, point1, capsuleCollider.radius, mask);

        // Store the first detected collider (if any)
        firstCollider = colliders.Length > 0 ? colliders[0] : null;

        return colliders;
    }

    public bool IsGrounded()
    {
        // Check if there's ground beneath the player
        return Physics.OverlapBox(boxCollider.transform.position, boxCollider.size / 2, Quaternion.identity, groundMask).Length > 0;
    }
}
