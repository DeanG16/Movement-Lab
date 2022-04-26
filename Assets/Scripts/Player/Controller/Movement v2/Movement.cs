using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    #region Public Variables
    public Transform orientationTransform;
    #endregion

    #region Events
    public System.Action<bool> playerJump;
    public System.Action<bool> playerSprint;
    public System.Action<bool> playerCrouch;
    public System.Action<bool> playerGrounded;
    public System.Action<bool> playerSlope;
    public System.Action<bool> playerSlide;
    #endregion

    #region Required Components
    private Rigidbody rb;
    private Player player;
    private StateHandler stateHandler;
    private InputManager inputManager;
    #endregion

    #region Control Modifier Variables
    private float airControl = 0.5f;
    private float slideControl = 0.25f;
    #endregion

    #region Speed/Force Variables
    private float baseMovementForce = 4000f;
    private float maximumMovementSpeed;
    private float walkingMovementSpeed = 7.5f;
    private float sprintingMovementSpeed = 11f;
    private float crouchingMovementSpeed = 3f;
    private float jumpForce = 700f;
    #endregion

    #region Physics Force Variables
    private float gravity = 1900f;
    #endregion

    private float maximumSlopeAngle = 50f;

    private void Awake() {
        Setup();
    }

    private void Update() {
        HandleInputs();
    }

    private void FixedUpdate() {
        PerformCollisionChecks();
        HandleGravity();
        HandleMovement();
    }

    private void HandleInputs() {
        if (stateHandler.IsGrounded) {
            HandleSpeedChanges();
            if (inputManager.CrouchPressed && !stateHandler.IsCrouching) {
                Crouch();
            } else if (!inputManager.CrouchPressed && stateHandler.IsCrouching) {
                UnCrouch();
            }
        }

        if (stateHandler.IsGrounded || stateHandler.IsSliding) {
            if (inputManager.JumpPressed) {
                Jump();
            }
        }
    }

    private void HandleMovement() {
        Vector2 playerInput = HandleInputLimiting();
        rb.AddForce(CalculateMovementVector(playerInput), ForceMode.Force);
    }

    #region Physics Functions
    private void HandleSpeedChanges() {
        if (inputManager.SprintPressed) {
            maximumMovementSpeed = sprintingMovementSpeed;
            playerSprint(true);
            playerCrouch(false);
        } else if (inputManager.CrouchPressed) {
            maximumMovementSpeed = crouchingMovementSpeed;
            playerCrouch(true);
            playerSprint(false);
        } else {
            maximumMovementSpeed = walkingMovementSpeed;
            playerCrouch(false);
            playerSprint(false);
        }
    }
    private void HandleGravity() {
        if (!stateHandler.IsGrounded) {
            if (stateHandler.IsSliding) {
                rb.AddForce(GetSlopeDownwards() * gravity * Time.fixedDeltaTime);
            } else {
                rb.AddForce(Vector3.down * gravity * Time.fixedDeltaTime);
            }
        }
    }
    #endregion

    #region Action Functions
    private void Jump() {
        RaycastHit hitInfo;
        Physics.Raycast(orientationTransform.transform.position, Vector3.down * 2f, out hitInfo, stateHandler.groundLayer);

        // Reset y velocity on slope to prevent dampened jumping.
        if (stateHandler.IsOnSlope || stateHandler.IsSliding) {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }

        rb.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
        playerJump(true);
    }

    private void Crouch() {
        playerCrouch(true);
    }

    private void UnCrouch() {
        playerCrouch(false);
    }
    #endregion

    #region Counter Movement Functions
    private Vector2 HandleInputLimiting() {
        Vector2 magnitudes = MovementUtils.LookDirectionVelocity(rb.velocity, orientationTransform.eulerAngles.y);

        float xInput = inputManager.movementInput.x;
        float zInput = inputManager.movementInput.y;

        // If the rigidbody is moving past the maximum speed in a direction, cancel out the player's input in that direction.
        if ((xInput > 0 && magnitudes.x > maximumMovementSpeed) ||
            (xInput < 0 && magnitudes.x < 0f - maximumMovementSpeed)) {
            xInput = 0f;
        }

        if ((zInput > 0 && magnitudes.y > maximumMovementSpeed) ||
            (zInput < 0 && magnitudes.y < 0f - maximumMovementSpeed)) {
            zInput = 0f;
        }

        // Calculate the hypotenuse of diagonal movement, and cancel out player input if over the max speed.
        if (zInput != 0 && xInput != 0 && (Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2f) + Mathf.Pow(rb.velocity.z, 2f)) > maximumMovementSpeed)) {
            zInput = 0f;
            xInput = 0f;
        }

        return new Vector2(xInput, zInput);
    }
    #endregion

    #region Vector Functions
    Vector3 GetVectorOnSlope(Vector3 vector) {
        RaycastHit hitInfo;
        Physics.Raycast(orientationTransform.transform.position, Vector3.down, out hitInfo, 1f, stateHandler.groundLayer);
        return Vector3.ProjectOnPlane(vector, hitInfo.normal);
    }

    Vector3 CalculateMovementVector(Vector2 inputs) {
        Vector3 movementDirection = orientationTransform.forward * inputs.y + orientationTransform.right * inputs.x;

        if (stateHandler.IsGrounded) {
            if (stateHandler.IsOnSlope) {
                return GetVectorOnSlope(movementDirection) * baseMovementForce * Time.fixedDeltaTime;
            }
            return movementDirection * baseMovementForce * Time.fixedDeltaTime;
        } else if (stateHandler.IsSliding) {
            return GetVectorOnSlope(movementDirection) * baseMovementForce * slideControl * Time.fixedDeltaTime;
        } else {
            return movementDirection * baseMovementForce * airControl * Time.fixedDeltaTime;
        }
    }

    Vector3 GetSlopeDownwards() {
        RaycastHit hitInfo;
        Physics.Raycast(orientationTransform.transform.position, Vector3.down, out hitInfo, 1f, stateHandler.groundLayer);

        Vector3 acrossSlope = Vector3.Cross(Vector3.ProjectOnPlane(transform.up, hitInfo.normal), hitInfo.normal);
        return Vector3.Cross(acrossSlope, hitInfo.normal);
    }

    bool IsNormalFloor(Vector3 normal) {
        return Vector3.Angle(Vector3.up, normal) <= maximumSlopeAngle;
    }
    #endregion

    #region Required Component Configuration
    private void Setup() {
        stateHandler = GetComponent<StateHandler>();
        inputManager = GetComponent<InputManager>();
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
    }
    #endregion

    private void PerformCollisionChecks() {
        RaycastHit hitInfo;
        if (Physics.SphereCast(orientationTransform.position + new Vector3(0f, 2f, 0f), 0.25f, Vector3.down, out hitInfo, 2f, stateHandler.groundLayer)) {
            if (stateHandler.groundLayer != (stateHandler.groundLayer | (1 << hitInfo.collider.gameObject.layer))) {
                return;
            }

            if (IsNormalFloor(hitInfo.normal)) {
                playerGrounded(true);
                playerSlide(false);

                if (Vector3.Angle(Vector3.up, hitInfo.normal) > 1f) {
                    playerSlope(true);
                } else {
                    playerSlope(false);
                }
            } else {
                playerGrounded(false);
                playerSlide(true);
            }
        } else {
            playerGrounded(false);
            playerSlide(false);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (stateHandler.groundLayer != (stateHandler.groundLayer | (1 << collision.collider.gameObject.layer))) {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++) {
            if (IsNormalFloor(collision.contacts[i].normal) && stateHandler.IsJumping) {
                playerJump(false);
            }
        }
    }
}