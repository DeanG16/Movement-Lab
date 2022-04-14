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

    PlayerMovement playerMovement;
    InputManager inputManager;

    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsOnSlope { get; private set; }
    public bool IsExitingSlide { get; private set; }
    public bool IsJumping { get; private set; }

    bool cancelGrounded;
    int cancellationTimer;
    float cancelDelay = 5f;

    private void Awake() {
        playerMovement = GetComponent<PlayerMovement>();
        inputManager = GetComponentInParent<InputManager>();
    }

    private void Update() {
        CheckIfSprinting();
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
                cancelGrounded = false;
                cancellationTimer = 0;

                if (Vector3.Angle(Vector3.up, hitInfo.normal) > 1f) {
                    IsOnSlope = true;
                } else {
                    IsOnSlope = false;
                }
                IsSliding = false;
            } else if (IsOnSlide(hitInfo.normal)) {
                IsSliding = true;
            }
        } else {
            IsSliding = false;
        }

        slopeAngleChange(Vector3.Angle(Vector3.up, hitInfo.normal));
        groundedChange(IsGrounded);
        slopeChange(IsOnSlope);
        slidingChange(IsSliding);
    }

    private bool IsOnGround(Vector3 normal) {
        return Vector3.Angle(Vector3.up, normal) < playerMovement.maxSlopeAngle;
    }

    private bool IsOnSlide(Vector3 normal) {
        float surfaceAngle = Vector3.Angle(Vector3.up, normal);
        if (surfaceAngle > playerMovement.maxSlopeAngle) {
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
                IsGrounded = false;
                IsOnSlope = false;
                groundedChange(IsGrounded);
                slopeChange(IsOnSlope);
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

}
