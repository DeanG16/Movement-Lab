using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateHandler : MonoBehaviour
{
    public LayerMask groundLayer;
    #region Required Components
    private Movement movement;
    private Rigidbody rb;
    #endregion

    #region Movement States
    public bool IsSprinting { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsOnSlope { get; private set; }
    public bool IsCrouching { get; private set; }
    #endregion

    #region Physics States
    public bool IsGrounded { get; private set; }
    #endregion

    private void Awake() {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody>();

        // Movement Events
        movement.playerJump += (value) => { SetJumping(value); };
        movement.playerSprint += (value) => { SetSprinting(value); };
        movement.playerCrouch += (value) => { SetCrouching(value); };
        movement.playerGrounded += (value) => { SetGrounded(value); };
        movement.playerSlope += (value) => { SetSlope(value); };
        movement.playerSlide += (value) => { SetSlide(value); };
    }

    private void FixedUpdate() {
        SetPhysicsParameters();
    }
  
    private void SetPhysicsParameters() {
        if (IsGrounded) {
            rb.drag = 5f;
            rb.useGravity = IsOnSlope ? false : true;
        } else if (IsSliding) {
            rb.drag = IsCrouching ? 0.75f : 1f;
            rb.useGravity = true;
        } else {
            rb.drag = 1f;
            rb.useGravity = true;
        }
    }

    #region State Setters
    private void SetJumping(bool value) {
        IsJumping = value;
    }

    private void SetSprinting(bool value) {
        IsSprinting = value;
    }

    private void SetCrouching(bool value) {
        IsCrouching = value;
    }

    private void SetGrounded(bool value) {
        IsGrounded = value;

        if(!value) {
            SetSlope(false);
        }
    }

    private void SetSlope(bool value) {
        IsOnSlope = value;
    }

    private void SetSlide(bool value) {
        IsSliding = value;
    }
    #endregion
}

