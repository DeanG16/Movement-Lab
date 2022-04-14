using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(StateManager))]
public class PlayerMovement : MonoBehaviour
{
    public System.Action<float> magnitudeChange;
    public System.Action<float> maxSpeedChange;

    public System.Action<bool> playerJumped;

    // Private Variables
    private Rigidbody rb;
    private InputManager inputManager;
    private StateManager stateManager;

    private Vector3 playerScale;
    private Vector3 crouchScale = new Vector3(1f, 0.65f, 1f);

    // Public Variables
    [Header("Orientation Transform")]
    [Tooltip("This will control the direction the rigidbody moves in")]
    public Transform orientation;

    [Header("Physics Settings")]
    public float airControl = 0.25f;
    public float slopeControl = 0.25f;
    public float slopeForce = 250f;
    public float maxSlopeAngle { get; private set; } = 50f;
    public float extraGravity = 2000f;
    public LayerMask groundLayer;

    [Header("Movement Settings")]
    public float movementSpeed = 3500f;
    public float walkSpeed = 6.5f;
    public float runSpeed = 10f;
    public float crouchSpeed = 3f;
    public float slidingSpeed = 30f;
    public float maxAirSpeed = 20f;
    public float maxSpeed = 6.5f;

    [Header("Jump Settings")]
    public float jumpForce = 700f;

    private void Awake() {
        inputManager = GetComponentInParent<InputManager>();
        stateManager = GetComponent<StateManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (stateManager.IsGrounded || stateManager.IsOnSlope || stateManager.IsSliding) {
            if (inputManager.JumpPressed) {
                Jump();
            }
        }
    }

    void FixedUpdate() {
        SetMovementSpeed();
        ApplyPhysics();
        ApplyMovement();
    }

    void SetMovementSpeed() {
        if (stateManager.IsGrounded) {
            if (stateManager.IsCrouching) {
                this.maxSpeed = crouchSpeed;
                return;
            }

            if (stateManager.IsSprinting) {
                this.maxSpeed = this.runSpeed;
            } else {
                this.maxSpeed = this.walkSpeed;
            }
        }
        
        maxSpeedChange(maxSpeed);
    }

    void ApplyMovement() {
        Vector2 calculatedInput = LimitMovementSpeed();
    
        Vector3 velocity;
        Vector3 direction = orientation.forward * calculatedInput.y + orientation.right * calculatedInput.x;

        if (stateManager.IsGrounded) {
            velocity = direction * movementSpeed * Time.fixedDeltaTime;
            if (stateManager.IsOnSlope) {
                velocity = GetVectorOnSlope(direction) * movementSpeed * Time.fixedDeltaTime;
            }
        } else if (stateManager.IsSliding) {
            velocity = GetVectorOnSlope(direction) * movementSpeed * slopeControl * Time.fixedDeltaTime;
        } else {
            velocity = direction * movementSpeed * airControl * Time.fixedDeltaTime;
        }

        rb.AddForce(velocity);

        magnitudeChange(rb.velocity.magnitude);
    }

    public Vector2 LimitMovementSpeed() {
        // Get angle between current orientation (Y axis) and rigidbody's velocity direction, converting to degrees.
        // This is so we limit movement in the correct direction and not the global rotation of the player.
        float velFromOrientationAngle = Mathf.DeltaAngle(orientation.transform.eulerAngles.y, Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg);
        float magnitude = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;

        // Calculate X and Z magnitude, converting degrees back to radians and account for Unity's clockwise Y rotation.
        // > X -> Sin(Theta) = Cos(90 - Angle).
        // > Z -> Cos(Theta) = Sin(90 - Angle).
        float xMag = magnitude * Mathf.Sin(velFromOrientationAngle * Mathf.Deg2Rad);
        float zMag = magnitude * Mathf.Cos(velFromOrientationAngle * Mathf.Deg2Rad);

        Vector2 calculatedMagnitude = new Vector2(xMag, zMag);

        float xInput = inputManager.movementInput.x;
        float zInput = inputManager.movementInput.y;

        // If the rigidbody is moving past the maximum speed in a direction, cancel out the player's input in that direction.
        if ((xInput > 0 && calculatedMagnitude.x > maxSpeed) ||
            (xInput < 0 && calculatedMagnitude.x < 0f - maxSpeed)) {
            xInput = 0f;
        }

        if ((zInput > 0 && calculatedMagnitude.y > maxSpeed) ||
            (zInput < 0 && calculatedMagnitude.y < 0f - maxSpeed)) {
            zInput = 0f;
        }

        // Calculate the hypotenuse of diagonal movement, and cancel out player input if over the max speed.
        if (zInput != 0 && xInput != 0 && (Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2f) + Mathf.Pow(rb.velocity.z, 2f)) > maxSpeed)) {
            zInput = 0f;
            xInput = 0f;
        }

        return new Vector2(xInput, zInput);
    }

    void Jump() {
        RaycastHit hitInfo;
        Physics.Raycast(orientation.transform.position, Vector3.down * 2f, out hitInfo, groundLayer);

        // Reset y velocity on slope to prevent dampened jumping.
        if (stateManager.IsOnSlope || stateManager.IsSliding) {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }

        rb.AddForce(hitInfo.normal * jumpForce * 0.1f * Time.fixedDeltaTime, ForceMode.Impulse);
        rb.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    void ApplyPhysics() {
        // Grounded Physics
        if (stateManager.IsGrounded && !stateManager.IsOnSlope) {
            rb.drag = 5f;
            rb.useGravity = true;
            return;
        }

        // Slope Physics
        if (stateManager.IsGrounded && stateManager.IsOnSlope) {
            rb.drag = 5f;
            rb.useGravity = false;
            ApplySlopeForce();
            return;
        }

        // Sliding Physics
        if (stateManager.IsSliding) {
            rb.drag = 1f;
            rb.useGravity = true;
            ApplySlopeForce();
            ApplyGravity(GetSlopeDownwards());
            return;
        }

        // Airborne Physics
        if (!stateManager.IsGrounded && !stateManager.IsOnSlope && !stateManager.IsSliding) {
            rb.drag = 0f;
            rb.useGravity = true;
            ApplyGravity(Vector3.down);
            return;
        }
    }

    void ApplyGravity(Vector3 direction, float additionalForce = 0f, ForceMode mode = ForceMode.Force) {
        Vector3 forceDirection = additionalForce != 0f ? direction * additionalForce : direction * extraGravity;
        rb.AddForce(forceDirection * Time.fixedDeltaTime, mode);
    }

    void ApplySlopeForce() {
        // Add force to keep the player on the slope.
        RaycastHit slopeHit;
        if (Physics.Raycast(orientation.transform.position, Vector3.down, out slopeHit, 1.5f, groundLayer)) {
            rb.AddForce(-slopeHit.normal * slopeForce * Time.fixedDeltaTime);
        }
    }

    /* Utility Methods */
    Vector3 GetVectorOnSlope(Vector3 vector) {
        RaycastHit hitInfo;
        Physics.Raycast(orientation.transform.position, Vector3.down, out hitInfo, 1f, groundLayer);
        return Vector3.ProjectOnPlane(vector, hitInfo.normal);
    }

    Vector3 GetSlopeDownwards() {
        RaycastHit hitInfo;
        Physics.Raycast(orientation.transform.position, Vector3.down, out hitInfo, 1f, groundLayer);

        Vector3 acrossSlope = Vector3.Cross(Vector3.ProjectOnPlane(transform.up, hitInfo.normal), hitInfo.normal);
        return Vector3.Cross(acrossSlope, hitInfo.normal);
    }
}
