using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StateManager))]
public class PlayerMovement : MonoBehaviour
{
    public System.Action<float> magnitudeChange;
    public System.Action<float> maxSpeedChange;

    // Private Variables
    private Rigidbody rb;
    private InputManager inputManager;
    private StateManager stateManager;

    public float maxSpeed = 6.5f;

    private Vector3 playerScale;
    private Vector3 crouchScale = new Vector3(1f, 0.65f, 1f);

    // Public Variables
    [Header("Orientation Transform")]
    [Tooltip("This will control the direction the rigidbody moves in")]
    public Transform orientation;

    [Header("Physics Settings")]
    public float airControl = 0.25f;
    public float slopeControl = 0.25f;
    public float slopeForce = 5f;
    public float maxSlopeAngle { get; private set; } = 50f;
    public float extraGravity = 5f;
    public LayerMask groundLayer;

    [Header("Movement Settings")]
    public float movementSpeed = 3500f;
    public float walkSpeed = 6.5f;
    public float runSpeed = 10f;
    public float crouchSpeed = 3f;
    public float slidingSpeed = 50f;

    [Header("Jump Settings")]
    public float jumpForce;

    private void Awake() {
        inputManager = GetComponent<InputManager>();
        stateManager = GetComponentInParent<StateManager>();
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
        if (stateManager.IsGrounded || stateManager.IsOnSlope) {
            SetMovementSpeed();
            if (inputManager.JumpPressed) {
                Jump();
            }
        }
    }

    void FixedUpdate() {
        ApplyPhysics();
        ApplyMovement();
    }

    void Jump() {
        RaycastHit hitInfo;
        Physics.Raycast(orientation.transform.position, Vector3.down * 2f, out hitInfo, groundLayer);

        if (stateManager.IsOnSlope) {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }

        rb.AddForce(hitInfo.normal * jumpForce * 0.1f  * Time.fixedDeltaTime, ForceMode.Impulse);
        rb.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
        Debug.DrawRay(transform.position, Vector3.up * 2f, Color.yellow, 5f);
    }

    /*void StartCrouch() {
        if (Mathf.Abs(Vector3.Distance(transform.localScale, crouchScale)) > 0.01f && !inputManager.CrouchPressed) {
            transform.localScale = Vector3.Lerp(transform.localScale, crouchScale, 0.05f);
        }
    }

    void StopCrouch() {
        if (Mathf.Abs(Vector3.Distance(transform.localScale, playerScale)) > 0.01f && inputManager.CrouchPressed) {
            transform.localScale = Vector3.Lerp(transform.localScale, playerScale, 0.05f);
        }
    }*/

    void LimitSpeed() {
        // Limiting forward and side speed to current max
        Vector2 vel = new Vector2(rb.velocity.x, rb.velocity.z);
        if (vel.magnitude >= maxSpeed) {
            float yVel = rb.velocity.y;
            Vector3 limitedSpeed = vel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedSpeed.x, yVel, limitedSpeed.y);
        }
    }

    void SetMovementSpeed() {
        if ((stateManager.IsGrounded || stateManager.IsOnSlope) && !stateManager.IsSliding) {
            if (stateManager.IsCrouching) {
                maxSpeed = crouchSpeed;
            } else if (stateManager.IsSprinting) {
                maxSpeed = runSpeed;
            } else {
                maxSpeed = walkSpeed;
            }
        } else if (stateManager.IsOnSlope && stateManager.IsSliding) {
            maxSpeed = slidingSpeed;
        } else {
            maxSpeed = walkSpeed;
        }

        maxSpeedChange(maxSpeed);
    }

    void ApplyMovement() {
        Vector3 direction = (orientation.forward * inputManager.movementInput.y + orientation.right * inputManager.movementInput.x).normalized;
        Vector3 velocity;
        if (stateManager.IsGrounded) {
            velocity = direction * movementSpeed * Time.fixedDeltaTime;
        } else if (stateManager.IsOnSlope && !stateManager.IsSliding) {
            velocity = GetVectorOnSlope(direction) * movementSpeed * Time.fixedDeltaTime;
        } else if (stateManager.IsSliding) {
            velocity = direction * movementSpeed * airControl * Time.fixedDeltaTime;
        } else {
            velocity = direction * movementSpeed * airControl * Time.fixedDeltaTime;
        }

        if (inputManager.movementInput.x != 0f || inputManager.movementInput.y != 0f) {
            rb.AddForce(velocity);
        }

        LimitSpeed();
        magnitudeChange(rb.velocity.magnitude);
    }

    void ApplyPhysics() {
        // Grounded Physics
        if (stateManager.IsGrounded) {
            rb.drag = 5f;
            rb.useGravity = true;
        }

        // Slope Physics
        if (!stateManager.IsGrounded && stateManager.IsOnSlope && !stateManager.IsSliding) {
            rb.drag = 5f;
            rb.useGravity = false;

            ApplySlopeForce();
        }
        
        // Airborne Physics
        if (!stateManager.IsGrounded && !stateManager.IsOnSlope && !stateManager.IsSliding) {
            rb.drag = 0f;
            rb.useGravity = true;
            ApplyGravity(Vector3.down);
        }

        // Sliding Physics
        if (!stateManager.IsGrounded && stateManager.IsOnSlope && stateManager.IsSliding) {
            rb.drag = 1f;
            rb.useGravity = true;
            ApplySlopeForce();
            ApplyGravity(GetSlopeDownwards());
        }
    }

    void ApplyGravity(Vector3 direction, float additionalForce = 0f) {
        Vector3 forceDirection = additionalForce != 0f ? direction * extraGravity * additionalForce : direction * extraGravity;
        rb.AddForce(forceDirection * Time.fixedDeltaTime);
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
