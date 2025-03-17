using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerController : NetworkBehaviour
{
    // Enum to represent the player's state (on the ground or in midair)
    private enum PlayerState { Grounded, Midair }
    private PlayerState playerState;

    [Header("Components")]
    [SerializeField] private PlayerDetection playerDetection; // Reference to PlayerDetection to check ground and obstacles

    [Header("Elements")]
    [SerializeField] private BoxCollider groundDetector; // Used to detect if the player is on the ground
    [SerializeField] private LayerMask groundMask; // Mask for detecting ground layer

    [Header("Settings")]
    [SerializeField] private float moveSpeed; // Speed of horizontal movement
    [SerializeField] private float jumpSpeed; // Vertical speed when jumping

    public float XVelocity { get; private set; } // Tracks the horizontal velocity for syncing across the network
    private float yVelocity; // Tracks the vertical velocity (used for gravity and jumping)

    [Header("Actions")]
    public Action onJumpStart; // Action triggered when the jump starts
    public Action onFallStart; // Action triggered when the player starts falling
    public Action onLand; // Action triggered when the player lands on the ground

    private void Start()
    {
        // Player starts on the ground, so set the initial state to grounded
        playerState = PlayerState.Grounded;
    }

    private void Update()
    {
        // If the player doesn't own this object (in a networked multiplayer game), skip processing
        if (!IsOwner) return;

        // Handle horizontal and vertical movement each frame
        MoveHorizontal();
        MoveVertical();
    }

    private void MoveHorizontal()
    {
        // Get movement input from the player
        Vector2 moveVector = InputManager.instance.GetMoveVector();

        // Update horizontal velocity on the network (sync across all clients)
        UpdateXVelocityRpc(Mathf.Abs(moveVector.x));

        // Manage which direction the player is facing based on movement
        ManageFacing(moveVector.x);

        // Apply movement speed to the input
        moveVector.x *= moveSpeed;

        // Calculate the target horizontal position
        float targetX = transform.position.x + moveVector.x * Time.deltaTime;
        Vector2 targetPosition = transform.position.With(x: targetX);

        // If the target position is valid (no obstacle in the way), move the player
        if (playerDetection.CanGoThere(targetPosition, out Collider firstCollider))
        {
            transform.position = targetPosition;
        }
    }

    private void ManageFacing(float xVelocity)
    {
        // Adjust the player's facing direction (left or right) based on the movement direction
        float facing = xVelocity != 0 ? Mathf.Sign(xVelocity) : transform.localScale.x;
        transform.localScale = transform.localScale.With(x: facing);
    }

    private void MoveVertical()
    {
        // Depending on the player's current state, move vertically either as grounded or in midair
        switch (playerState)
        {
            case PlayerState.Grounded:
                MoveVerticalGrounded(); // Handle movement when grounded
                break;

            case PlayerState.Midair:
                MoveVerticalMidAir(); // Handle movement while in midair
                break;
        }
    }

    private void MoveVerticalGrounded()
    {
        // If the player is no longer grounded, start falling
        if (!playerDetection.IsGrounded())
        {
            StartFalling();
        }
    }

    private void MoveVerticalMidAir()
    {
        // Calculate the target vertical position based on the current yVelocity (affected by gravity)
        float targetY = transform.position.y + yVelocity * Time.deltaTime;
        Vector3 targetPosition = transform.position.With(y: targetY);

        // If falling, check if there is an obstacle (e.g., the ground) blocking the way
        if (!playerDetection.CanGoThere(targetPosition, out Collider firstCollider))
        {
            // Get the minimum Y value of the collider (the highest point of the obstacle)
            float minY = firstCollider.ClosestPoint(transform.position).y;

            // Raycast downward to check for the ground and adjust the player's position accordingly
            Physics.Raycast(groundDetector.transform.position, Vector3.down, out RaycastHit hit, 1, groundMask);

            if (hit.collider != null)
            {
                // If the raycast hits something, adjust the target Y position to match the ground level
                targetPosition.y = minY;
            }
            else
            {
                // If no ground is detected, set a fallback value to keep the player from falling too far
                float maxY = firstCollider.ClosestPoint(transform.position).y;
                targetPosition.y = maxY - 2.4f; // Prevent clipping into the ground
                yVelocity = 0; // Stop falling if there's no more space to move
            }
        }
        else
        {
            // Apply gravity to the player while in the air
            yVelocity += Physics.gravity.y * Time.deltaTime;
        }

        transform.position = targetPosition; // Update the player's position

        // If the player is grounded, transition back to grounded state
        if (playerDetection.IsGrounded())
        {
            Land();
        }
    }

    private void StartFalling()
    {
        // Change the state to Midair and trigger the fall start event
        playerState = PlayerState.Midair;
        onFallStart?.Invoke();
    }

    private void Land()
    {
        // Change the state to Grounded and reset vertical velocity
        playerState = PlayerState.Grounded;
        yVelocity = 0;
        onLand?.Invoke(); // Trigger the land event
    }

    public void Jump()
    {
        // Transition to midair state and apply the jump speed
        playerState = PlayerState.Midair;
        yVelocity = jumpSpeed;
        onJumpStart?.Invoke(); // Trigger the jump start event
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateXVelocityRpc(float xSpeed)
    {
        // Sync the horizontal velocity across the network for all clients
        XVelocity = xSpeed;
    }
}
