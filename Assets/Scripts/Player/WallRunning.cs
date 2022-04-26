using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class WallRunning : MonoBehaviour
{
    public System.Action<bool> playerWallRunning;
    public System.Action<bool> playerWallRunJumped;

    PlayerMovement playerMovement;
    InputManager inputManager;
    StateManager stateManager;
    Rigidbody rb;

    // Wall detection variables
    bool wallLeft = false;
    bool wallRight = false;
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    Vector3 activeWallRunNormal;
    Vector3 wallRunDirection;

    [Header("Wall Run Settings")]
    float maxWallDistance = 0.7f;
    float wallRunSpeed = 4000f;
    float wallRunGravity = 750f;
    float wallAttachForce = 3f;
    float minimumSpeed = 2f;


    private void Awake() {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        inputManager = GetComponentInParent<InputManager>();
        stateManager = GetComponent<StateManager>();
    }
  
    void Update()
    {
        HandleJump();
    }

    private void FixedUpdate() {
        CheckForWalls();
        HandleWallRunning();
        if (stateManager.IsWallRunning) {
            ApplyWallRunPhysics();
        }       
    }

    void StartWallRunning() {
        ResetVerticalVelocity();
        playerWallRunning(true);
    }

    void StopWallRunning() {
        playerWallRunning(false);
    }

    void HandleWallRunning() {
        float currentSpeed = Mathf.Abs(new Vector2(rb.velocity.x, rb.velocity.z).magnitude);
        // If there are no walls to run on, cancel wall running and return early.
        if (currentSpeed < minimumSpeed || (!stateManager.IsWallRunning && !stateManager.CanWallRun)) {
            StopWallRunning();
            return;
        }

        if (!stateManager.IsGrounded && !stateManager.IsSliding && !stateManager.IsWallRunning) {
            // Left Hand Wall Run
            if (wallLeft) {
                StartWallRunning();
                activeWallRunNormal = leftWallHit.normal;
                // Right Hand Wall Run
            } else if (wallRight) {
                StartWallRunning();
                activeWallRunNormal = rightWallHit.normal;
            } else {
                StopWallRunning();
            }
        }
    }

    void ApplyWallRunPhysics() {
        SetWallRunDirection();
        ApplyWallForce(activeWallRunNormal);
        rb.AddForce(wallRunDirection.normalized * wallRunSpeed * Time.fixedDeltaTime);
        rb.AddForce(Vector3.down * wallRunGravity * Time.fixedDeltaTime);
    }

    void HandleJump() {
        if (inputManager.JumpPressed && stateManager.CanJump && stateManager.IsWallRunning) {
            if (wallLeft) {
                WallJump(leftWallHit.normal, inputManager.movementInput.x > 0f);
                StopWallRunning();
            } else if (wallRight) {
                WallJump(rightWallHit.normal, inputManager.movementInput.x < 0f);
                StopWallRunning();
            }
        }
    }

    void WallJump(Vector3 normal, bool boost) {
        // Update Events
        playerWallRunJumped(true);
        playerWallRunning(false);

        ResetVerticalVelocity();

        float jumpModifier = boost ? 0.5f : 0.2f;
        // Apply Forces
        rb.AddForce(normal * playerMovement.jumpForce * jumpModifier * Time.fixedDeltaTime, ForceMode.Impulse);
        rb.AddForce(Vector3.up * playerMovement.jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    // Utility Functions
    void SetWallRunDirection() {
        RaycastHit hitInfo;
        Vector3 origin = transform.position + new Vector3(0f, 1f, 0f);
        Vector3 direction = wallLeft ? -playerMovement.orientation.right : playerMovement.orientation.right;
        if (Physics.Raycast(origin, direction * 2f, out hitInfo) ) {
            Vector3 acrossSlope = Vector3.Cross(Vector3.ProjectOnPlane(-playerMovement.orientation.forward, hitInfo.normal), hitInfo.normal);
            wallRunDirection = Vector3.Cross(acrossSlope, hitInfo.normal);
        }
    }

    // Physics Functions
    void ResetVerticalVelocity() {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }

    void ApplyWallForce(Vector3 normal) {
        rb.AddForce(-normal * wallAttachForce);
    }

    // Check Functions
    void CheckForWalls() {
        Vector3 origin = transform.position + new Vector3(0f, 1f, 0f);

        wallLeft = Physics.SphereCast(origin, 0.1f, -playerMovement.orientation.right, out leftWallHit, maxWallDistance) && WallIsRunnable(leftWallHit);
        wallRight = Physics.SphereCast(origin, 0.1f, playerMovement.orientation.right, out rightWallHit, maxWallDistance) && WallIsRunnable(rightWallHit);

        if(!wallLeft && !wallRight) {
            StopWallRunning();
        }
    }

    bool WallIsRunnable(RaycastHit hitInfo) {
        return hitInfo.collider.gameObject.tag.Contains("WallRunnable");
    }
}
