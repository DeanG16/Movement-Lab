using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Movement : MonoBehaviour
{
    #region Public Variables
    public Transform orientationTransform;
    #endregion

    #region Events
    public System.Action<bool> playerJump;
    public System.Action<bool> playerCanJump;
    public System.Action<bool> playerSprint;
    public System.Action<bool> playerCrouch;
    public System.Action<bool> playerCrouchChange;
    public System.Action<bool> playerGrounded;
    public System.Action<bool> playerSlope;
    public System.Action<bool> playerSlide;
    #endregion

    #region Required Components
    private Rigidbody rb;
    private StateHandler stateHandler;
    private InputManager inputManager;
    private AudioSource audioSource;
    #endregion

    #region Control Modifier Variables
    private float airControl = 0.4f;
    private float slideControl = 0.25f;
    [SerializeField]
    [Tooltip("Multiplier for movement in the opposite direction the player is currently moving in.")]
    private float counterMovementMultiplier = 0.17f;
    [SerializeField]
    [Tooltip("The minimimum magnitude the player must reach before counter movement takes hold.")]
    private float counterMovementThreshold = 0.2f;
    #endregion

    #region Speed/Force Variables
    private float baseMovementForce = 3000f;
    private float crouchingMovementForce = 1700f;
    private float movementForce = 3000f;
    private float maximumMovementSpeed;
    private float walkingMovementSpeed = 7.5f;
    private float sprintingMovementSpeed = 11f;
    private float crouchingMovementSpeed = 3f;
    private float jumpForce = 750f;
    #endregion

    #region Presets
    public LayerMask groundLayer;
    private Vector3 playerBodyScale;
    private Vector3 crouchBodyScale = new Vector3(1f, 0.65f, 1f);
    #endregion

    #region Physics Force Variables
    private float gravity = 1900f;
    #endregion

    #region Threshholds
    private float maximumSlopeAngle = 50f;
    #endregion

    #region Footstep and Audio Variables
    public AudioClip[] footStepFx;
    public AudioClip[] jumpStartFx;
    public AudioClip[] jumpEndFx;
    private Vector3 lastPosition;
    private float distanceTravelled = 0f;
    private int lastIndex;
    #endregion

    #region Debugging
    private bool isPaused = false;
    #endregion

    private void Awake() {
        Setup();
        audioSource.clip = footStepFx[0];
    }

    private void Update() {
        if (!isPaused) {
            HandleInputs();
        }
        HandleFootsteps();
    }

    private void FixedUpdate() {
        PerformCollisionChecks();
        HandleGravity();
        PerformCounterMovement();

        if (!isPaused) {
            HandleMovement();
        }
    }

    private void HandleInputs() {
        if (inputManager.CrouchPressed) {
            if (stateHandler.IsGrounded || stateHandler.IsSliding) {
                maximumMovementSpeed = crouchingMovementSpeed;
            }
            Crouch();
        } else if (!inputManager.CrouchPressed && stateHandler.IsCrouching && CanUncrouch()) {
            UnCrouch();
        }

        // Grounded or Sliding states
        if (stateHandler.IsGrounded || stateHandler.IsSliding) {

            // Either state.
            if (inputManager.JumpPressed && stateHandler.CanJump && CanUncrouch()) {
                Jump();
            }

            // Ground only.
            if (stateHandler.IsGrounded) {
                // Sprint/Walk speed
                if (!stateHandler.IsCrouching ) {
                    if(inputManager.SprintPressed) {
                        maximumMovementSpeed = sprintingMovementSpeed;
                        playerSprint(true);
                    } else {
                        maximumMovementSpeed = walkingMovementSpeed;
                        playerSprint(false);
                    }
                }
            }

            // Slide only.
            if (stateHandler.IsSliding) {}
        }
    }

    #region Audio Functions
    private void HandleFootsteps() {
        distanceTravelled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;
        if (distanceTravelled > 3f && (inputManager.movementInput.x != 0f || inputManager.movementInput.y != 0f) 
            && !stateHandler.IsJumping && !stateHandler.IsCrouching && stateHandler.IsGrounded) {
            audioSource.clip = GetRandomSound(footStepFx);
            audioSource.Play();
            distanceTravelled = 0f;
        }
    }

    private void PlayJumpTakeOff() {
        audioSource.clip = GetRandomSound(jumpStartFx);
        audioSource.Play();
    }

    private void PlayJumpLanding() {
        audioSource.clip = GetRandomSound(jumpEndFx);
        audioSource.Play();
    }

    AudioClip GetRandomSound(AudioClip[] source) {
        int randomIndex = Random.Range(0, source.Length - 1);
        if (randomIndex == lastIndex) {
            randomIndex = Random.Range(0, source.Length - 1);
        }
        lastIndex = randomIndex;
        return source[randomIndex];
    }
    #endregion

    #region Physics Functions
    private void HandleMovement() {
        Vector2 playerInput = HandleInputLimiting();
        rb.AddForce(CalculateMovementVector(playerInput), ForceMode.Force);
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
        playerJump(true);
        playerCanJump(false);

        PlayJumpTakeOff();
        rb.isKinematic = false;

        RaycastHit hitInfo;
        Physics.Raycast(orientationTransform.transform.position, Vector3.down * 2f, out hitInfo, groundLayer);

        // Reset y velocity on slope to prevent dampened jumping.
        if (stateHandler.IsOnSlope || stateHandler.IsSliding) {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
        float force = stateHandler.IsCrouching ? jumpForce * 0.8f : jumpForce;
        rb.AddForce(Vector3.up * force * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    private void Crouch() {
        playerCrouch(true);
        if (stateHandler.IsGrounded) {
            movementForce = crouchingMovementForce;
        }

        // Cancel sprinting
        if (stateHandler.IsSprinting) {
            playerSprint(false);
        }

        if (Mathf.Abs(Vector3.Distance(transform.localScale, crouchBodyScale)) > 0.01f) {
            if (stateHandler.IsGrounded || stateHandler.IsSliding) {
                rb.AddForce(Vector3.down * 10f);
            }
            playerCrouchChange(true);
            transform.localScale = Vector3.Lerp(transform.localScale, crouchBodyScale, Time.deltaTime / 0.04f);
        } else {
            playerCrouchChange(false);
        }
    }

    private void UnCrouch() {
        movementForce = baseMovementForce;

        if (Mathf.Abs(Vector3.Distance(transform.localScale, playerBodyScale)) > 0.01f) {
            playerCrouchChange(true);
            transform.localScale = Vector3.Lerp(transform.localScale, playerBodyScale, Time.deltaTime / 0.04f);
        } else {
            playerCrouch(false);
            playerCrouchChange(false);
        }
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

    private void PerformCounterMovement() {
        Vector2 magnitude = MovementUtils.LookDirectionVelocity(rb.velocity, orientationTransform.eulerAngles.y);
        if (inputManager.movementInput.x == 0f && inputManager.movementInput.y == 0f && rb.velocity.magnitude < 1f && stateHandler.IsGrounded && !stateHandler.IsJumping && !stateHandler.CrouchStateChanging) {
            rb.isKinematic = true;
        } else {
            rb.isKinematic = false;
        }

        if (inputManager.movementInput.x == 0f) {
            Debug.Log(Mathf.Abs(magnitude.x));
            if (Mathf.Abs(magnitude.x) > 0.01f && Mathf.Abs(magnitude.x) < 10f) {
                rb.AddForce(movementForce * orientationTransform.right.normalized * 0.02f * (0f - magnitude.x) * counterMovementMultiplier * Time.fixedDeltaTime);
            }
        }

        if (inputManager.movementInput.y == 0f) {
            if (Mathf.Abs(magnitude.y) > 0.01f && Mathf.Abs(magnitude.y) < 10f) {
                rb.AddForce(movementForce * orientationTransform.forward.normalized * 0.02f * (0f - magnitude.y) * counterMovementMultiplier * Time.fixedDeltaTime);
            }
        }
    }
    #endregion

    #region Vector Functions
    Vector3 GetVectorOnSlope(Vector3 vector) {
        RaycastHit hitInfo;
        if (Physics.Raycast(orientationTransform.transform.position, Vector3.down, out hitInfo, 1f, groundLayer)) {
            return Vector3.ProjectOnPlane(vector.normalized, hitInfo.normal);
        } else {
            return vector;
        }
    }

    Vector3 CalculateMovementVector(Vector2 inputs) {
        Vector3 movementDirection = (orientationTransform.forward.normalized * inputs.y + orientationTransform.right.normalized * inputs.x);

        if (stateHandler.IsGrounded) {
            if (stateHandler.IsOnSlope) {
                return GetVectorOnSlope(movementDirection) * movementForce * Time.fixedDeltaTime;
            }
            return movementDirection * movementForce * Time.fixedDeltaTime;
        } else if (stateHandler.IsSliding) {
            return GetVectorOnSlope(movementDirection) * movementForce * slideControl * Time.fixedDeltaTime;
        } else {
            return movementDirection * movementForce * airControl * Time.fixedDeltaTime;
        }
    }

    Vector3 GetSlopeDownwards() {
        RaycastHit hitInfo;
        Physics.Raycast(orientationTransform.transform.position, Vector3.down, out hitInfo, 1f, groundLayer);

        Vector3 acrossSlope = Vector3.Cross(Vector3.ProjectOnPlane(transform.up, hitInfo.normal), hitInfo.normal);
        return Vector3.Cross(acrossSlope, hitInfo.normal);
    }

    bool IsNormalFloor(Vector3 normal) {
        return Vector3.Angle(Vector3.up, normal) <= maximumSlopeAngle;
    }
    #endregion

    #region Required Component Configuration
    private void Setup() {
        stateHandler = GetComponentInParent<StateHandler>();
        inputManager = GetComponentInParent<InputManager>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponentInChildren<AudioSource>();
        playerBodyScale = transform.localScale;

        lastPosition = transform.position;
        inputManager.pausedGame += (value) => { isPaused = value; };
    }
    #endregion

    #region Collision Handling
    private void PerformCollisionChecks() {
        RaycastHit hitInfo;
        if (Physics.SphereCast(orientationTransform.position + new Vector3(0f, 2f, 0f), 0.25f, Vector3.down, out hitInfo, 2f, groundLayer)) {
            if (groundLayer != (groundLayer | (1 << hitInfo.collider.gameObject.layer))) {
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
        if (groundLayer != (groundLayer | (1 << collision.collider.gameObject.layer))) {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++) {
                // Floor/Slope
            if (IsNormalFloor(collision.contacts[i].normal)) {
                playerCanJump(true);
                playerJump(false);
            }

            // Sliding
            if (Vector3.Angle(Vector3.up, collision.contacts[i].normal) > maximumSlopeAngle) {
                playerJump(false);
            }
        }
    }

    private bool CanUncrouch() {
        // Ensure we have enough space to stand up.
        // Raycast and Spherecast is used in the event the surface intersects the spherecast, which causes it to fail.
        RaycastHit hitInfo;
        if (Physics.Raycast(orientationTransform.position, Vector3.up * 2f, out hitInfo, 2f, groundLayer) ||
           Physics.SphereCast(orientationTransform.position, 0.2f, Vector3.up, out hitInfo, 2f, groundLayer)) {
            return false;
        } else {
            return true;
        }
    }
    #endregion
}