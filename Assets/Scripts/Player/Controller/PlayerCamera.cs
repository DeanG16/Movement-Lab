using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Player Camera")]
    public Camera playerCamera;

    [Header("Target Position")]
    public Transform targetPosition;

    [Header("Sensitivity Options")]
    [Range(1f, 100f)]
    public float xSensitivity = 15f;
    [Range(1f, 100f)]
    public float ySensitivity = 15f;

    [Header("Orientation")]
    public Transform orientation;

    // User Input
    private float xInput = 0f;
    private float yInput = 0f;

    // Camera Rotations
    private float xRotation, yRotation = 0f;


    private void Awake() {
        playerCamera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        // Get mouse inputs
        xInput = Input.GetAxis("Mouse X") * xSensitivity * 0.02f;
        yInput = Input.GetAxis("Mouse Y") * ySensitivity * 0.02f;

        // On the camera, y = x and x = y axis.
        yRotation += xInput;
        xRotation -= yInput;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        transform.position = targetPosition.position;
    }
}
