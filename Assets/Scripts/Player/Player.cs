using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(StateHandler))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerCamera))]
public class Player : MonoBehaviour
{
    #region Required Components
    private Rigidbody rb;
    private Movement movement;
    private StateHandler stateHandler;
    private CapsuleCollider capsuleCollider;
    private PlayerCamera playerCamera;
    #endregion

    private void Awake() {
        Setup();
    }
    #region Required Component Configuration
    private void Setup() {
        stateHandler = GetComponent<StateHandler>();
        movement = GetComponent<Movement>();
        
        rb = GetComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true;
        rb.useGravity = true;

        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.height = 2f;
        capsuleCollider.radius = 0.3f;
    }
    #endregion
}
