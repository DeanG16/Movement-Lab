using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public System.Action<float> magnitudeChange;
    public System.Action<float> slopeAngleChange;

    // Private Variables
    private Rigidbody rb;
    private Vector2 input;
    public bool grounded;
    public bool onSlope;
    public bool isJumping;
    public float maxSpeed = 6.5f;

    // Public Variables
    [Header("Orientation Transform")]
    [Tooltip("This will control the direction the rigidbody moves in")]
    public Transform orientation;

    [Header("Physics Settings")]
    public float airControl = 0.25f;
    public float slopeForce = 5f;
    public float extraGravity = 5f;
    public LayerMask groundLayer;

    [Header("Movement Settings")]
    public float movementSpeed = 3500f;
    public float walkSpeed = 6.5f;
    public float runSpeed = 10f;

    [Header("Jump Settings")]
    public float jumpForce;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        
        if (grounded) {
            this.maxSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Space) && grounded) {
            Jump();
        }

        if (grounded) {
            isJumping = false;
        } else {
            onSlope = false;
            slopeAngleChange(0f);
        }
    }

    void FixedUpdate() {
        GroundCheck();

        if (!grounded) {
            rb.useGravity = true;
            rb.drag = 0f;
            rb.AddForce(Vector3.down * extraGravity * Time.fixedDeltaTime);
        } else {
            rb.drag = 5f;
        }

        ApplyMovement();
    }

    void ApplyMovement() {
        Vector3 direction = (orientation.forward * input.y + orientation.right * input.x).normalized;
        Vector3 velocity;
        if (grounded) {
            if (onSlope && !isJumping) {
                velocity = GetVectorOnSlope(direction) * movementSpeed * Time.fixedDeltaTime;
            } else {
                velocity = direction * movementSpeed * Time.fixedDeltaTime;
            }
        } else {
            velocity = direction * movementSpeed * airControl * Time.fixedDeltaTime;
        }

        if (input.x != 0f || input.y != 0f) {
            rb.AddForce(velocity);
        }

        LimitSpeed();
        Debug.DrawRay(transform.position, rb.velocity.normalized * 2f, Color.red);
        magnitudeChange(rb.velocity.magnitude);
    }

    void Jump() {
        isJumping = true;

        RaycastHit hitInfo;
        Physics.Raycast(orientation.transform.position, Vector3.down * 2f, out hitInfo, groundLayer);

        if (onSlope) {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }

        rb.AddForce(hitInfo.normal * jumpForce * 0.1f  * Time.fixedDeltaTime, ForceMode.Impulse);
        rb.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
        Debug.DrawRay(transform.position, Vector3.up * 2f, Color.yellow, 5f);
    }

    void LimitSpeed() {
        // Limiting forward and side speed to current max
        Vector2 vel = new Vector2(rb.velocity.x, rb.velocity.z);
        if (vel.magnitude >= maxSpeed) {
            float yVel = rb.velocity.y;
            Vector3 limitedSpeed = vel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedSpeed.x, yVel, limitedSpeed.y);
        }
    }

    Vector3 GetVectorOnSlope(Vector3 vector) {
        RaycastHit hitInfo;
        Physics.Raycast(orientation.transform.position, Vector3.down, out hitInfo, 1f, groundLayer);
        return Vector3.ProjectOnPlane(vector, hitInfo.normal);
    }

    void GetInput() {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    void GroundCheck() {
        grounded = Physics.CheckSphere(orientation.transform.position, 0.25f, groundLayer);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(orientation.transform.position, 0.25f);
    }

    [System.Obsolete]
    private void OnCollisionStay(Collision collision) {
        foreach (ContactPoint contact in collision) {
            float contactAngle = Mathf.Abs(Vector3.Angle(Vector3.up, contact.normal));
            if(collision.gameObject.layer == 3) {
                slopeAngleChange(contactAngle);
                if (contactAngle != 0f && contactAngle <= 45f) {
                    onSlope = true;
                    rb.useGravity = false;
                    if (!isJumping) { 
                        rb.AddForce(-contact.normal * slopeForce * Time.fixedDeltaTime); 
                    }
                } else {
                    onSlope = false;
                    rb.useGravity = true;
                }
            }
        }
    }
}
