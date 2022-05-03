using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    InputManager inputManager;

    [Header("Player Camera")]
    public Camera playerCamera;
    [Header("Container Transform")]
    public Transform containerTransform;
    [Header("Target Position")]
    public Transform targetPosition;
    [Header("Orientation")]
    public Transform orientation;

    // Camera Rotations
    private float xRotation, yRotation = 0f;

    bool paused = false;

    void Awake() {
        inputManager = GetComponentInParent<InputManager>();
        inputManager.pausedGame += HandleMouseState;
    }

    void Start() {
        playerCamera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        if (!paused)
            HandleCameraMovement();
        }

    void HandleCameraMovement() {
        yRotation += inputManager.mouseXInput;
        xRotation -= inputManager.mouseYInput;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        containerTransform.position = targetPosition.position;
    }

    void HandleMouseState(bool isPaused) {
        paused = isPaused;

        if (paused) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        } else {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
