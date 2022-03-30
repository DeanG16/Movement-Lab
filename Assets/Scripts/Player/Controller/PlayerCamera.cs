using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Player Camera")]
    public Camera playerCamera;

    [Header("Sensitivity Options")]
    [Range(1f, 100f)]
    public int xSensitivity = 15;
    [Range(1f, 100f)]
    public int ySensitivity = 15;

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
        xInput = Input.GetAxisRaw("Mouse X") * (xSensitivity * 10f) * Time.deltaTime;
        yInput = Input.GetAxisRaw("Mouse Y") * (ySensitivity * 10f) * Time.deltaTime;
    }

    private void LateUpdate() {
        // On the camera, y = x and x = y axis.
        yRotation += xInput;
        xRotation -= yInput;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}
