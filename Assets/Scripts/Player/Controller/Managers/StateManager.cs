using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class StateManager : MonoBehaviour
{
    public System.Action<bool> groundedChange;
    public System.Action<float> slopeAngleChange;
    public System.Action<bool> slopeChange;
    public System.Action<bool> slidingChange;
    public System.Action<bool> exitingSlideChange;
    public System.Action<bool> wallRunningChange;
    public System.Action<bool> jumpingChange;

    PlayerMovement playerMovement;
    WallRunning wallRunning;
    InputManager inputManager;

    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsOnSlope { get; private set; }
    public bool IsExitingSlide { get; private set; }
    public bool IsJumping { get; private set; }
    public bool CanJump { get; private set; }
    public bool IsWallRunning { get; private set; }
    public bool CanWallRun { get; private set; }

    bool cancelGrounded;
    int cancellationTimer;
    float cancelDelay = 5f;

    private void Awake() {
        playerMovement = GetComponent<PlayerMovement>();
        inputManager = GetComponentInParent<InputManager>();
        wallRunning = GetComponent<WallRunning>();
    }

    private void Start() {
        wallRunning.playerWallRunning += (result) => { IsWallRunning = result; };
        playerMovement.playerJumped += (result) => {
            if (IsSliding) {
                IsExitingSlide = true;
            }

            IsJumping = true;
            CanJump = false;
            jumpingChange(IsJumping);
        };

        wallRunning.playerWallRunJumped += (result) => {
            IsJumping = true;
            CanJump = false;
            CanWallRun = true;
            jumpingChange(IsJumping);
        };
    }

    private void Update() {
        CheckIfSprinting();
        CheckIfCrouching();
    }

    private void FixedUpdate() {
        UpdateCollisionChecks();
        PerformCollisionChecks();
    }

    private void PerformCollisionChecks() {
        RaycastHit hitInfo;
        if (Physics.SphereCast(playerMovement.orientation.position + new Vector3(0f, 2f, 0f), 0.25f, Vector3.down, out hitInfo, 2f, playerMovement.groundLayer)) {
            if (playerMovement.groundLayer != (playerMovement.groundLayer | (1 << hitInfo.collider.gameObject.layer))) {
                return;
            }

            if (IsOnGround(hitInfo.normal)) {
                IsGrounded = true;
                IsWallRunning = false;

                cancelGrounded = false;
                cancellationTimer = 0;

                if (Vector3.Angle(Vector3.up, hitInfo.normal) > 1f) {
                    IsOnSlope = true;
                } else {
                    IsOnSlope = false;
                }

                IsSliding = false;
            } else if (IsOnSlide(hitInfo.normal)) {
                IsWallRunning = false;
                IsSliding = true;
            }
        } else {
            IsSliding = false;
        }

        wallRunningChange(IsWallRunning);
        slopeAngleChange(Vector3.Angle(Vector3.up, hitInfo.normal));
        groundedChange(IsGrounded);
        slopeChange(IsOnSlope);
        slidingChange(IsSliding);
    }

    private void OnCollisionEnter(Collision collision) {
        RaycastHit hitInfo;
        if (Physics.Raycast(playerMovement.orientation.position + new Vector3(0f, 0.25f, 0f), Vector3.down * 0.75f, out hitInfo, Mathf.Infinity, playerMovement.groundLayer)) {
            if (IsOnGround(hitInfo.normal) || IsOnSlide(hitInfo.normal)) {
                IsJumping = false;
                CanJump = true;
                CanWallRun = true;
            }
        } else if (IsWallRunning) {
            IsJumping = false;
            CanJump = true;
            CanWallRun = false;
        } else {
            CanWallRun = true;
        }

        jumpingChange(IsJumping);
    }

    private bool IsOnGround(Vector3 normal) {
        return Vector3.Angle(Vector3.up, normal) < playerMovement.maxSlopeAngle;
    }

    private bool IsOnSlide(Vector3 normal) {
        float surfaceAngle = Vector3.Angle(Vector3.up, normal);
        if (surfaceAngle > playerMovement.maxSlopeAngle && surfaceAngle < 89f ) {
            return true;
        }
        return false;
    }

    private void UpdateCollisionChecks() {
        if (!cancelGrounded) {
            cancelGrounded = true;
        } else {
            cancellationTimer++;
            if ((float)cancellationTimer > cancelDelay) {
                CancelGroundStates();
            }
        }
    }

    private void CheckIfSprinting() {
        if (inputManager.SprintPressed && (IsGrounded || IsOnSlope)) {
            IsSprinting = true;
        } else {
            IsSprinting = false;
        }
    }

    private void CheckIfCrouching() {
        if (inputManager.CrouchPressed) {
            IsCrouching = true;
        } else {
            IsCrouching = false;
        }
    }

    private void CancelGroundStates() {
        IsGrounded = false;
        IsOnSlope = false;
        groundedChange(IsGrounded);
        slopeChange(IsOnSlope);
    }
}
