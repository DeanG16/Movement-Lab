using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class StateManager : MonoBehaviour
{
    public System.Action<bool> groundedChange;
    public System.Action<float> slopeAngleChange;
    public System.Action<bool> slopeChange;
    public System.Action<bool> slidingChange;

    PlayerMovement playerMovement;
    InputManager inputManager;

    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsOnSlope { get; private set; }

    void Awake() {
        playerMovement = GetComponent<PlayerMovement>();
        inputManager = GetComponentInParent<InputManager>();
    }

    void Update() {
        CheckIfSprinting();
    }

    void OnCollisionStay(Collision collision) {
        ContactPoint[] contactPoints = collision.contacts;

        for (int i = 0; i < collision.contactCount; i++) {
            float contactNormalAngle = Mathf.Abs(Vector3.Angle(Vector3.up, contactPoints[i].normal));
            if (contactNormalAngle >= 90f || collision.gameObject.layer != 3) { continue; }

            slopeAngleChange(contactNormalAngle);

            if (Mathf.Abs(contactNormalAngle) == 0f) {
                IsGrounded = true;
                IsOnSlope = false;
                IsSliding = false;
            } else if (contactNormalAngle > 0f) {
                IsGrounded = false;
                IsOnSlope = true;

                if (Mathf.Abs(contactNormalAngle) > playerMovement.maxSlopeAngle) {
                    IsSliding = true;
                } else {
                    IsSliding = false;
                }
            }
        }
        groundedChange(IsGrounded);
        slidingChange(IsSliding);
        slopeChange(IsOnSlope);
    }

    void OnCollisionExit(Collision collision) {
        if (collision.gameObject.layer == 3) {
            SetAirborneState();
        }
    }

    void CheckIfSprinting() {
        if (inputManager.SprintPressed && (IsGrounded || IsOnSlope)) {
            IsSprinting = true;
        } else {
            IsSprinting = false;
        }
    }

    void SetAirborneState() {
        IsGrounded = false;
        IsOnSlope = false;
        IsSliding = false;

        groundedChange(IsGrounded);
        slidingChange(IsSliding);
        slopeChange(IsOnSlope);
    }
}
