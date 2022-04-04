using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public bool SprintPressed { get; private set; }
    public bool CrouchPressed { get; private set; }
    public bool JumpPressed { get; private set; }
    public Vector2 movementInput { get; private set; }

    // Keyboard Controls
    public KeyCode sprintKey { get; set; } = KeyCode.LeftShift;
    public KeyCode crouchKey { get; set; } = KeyCode.LeftControl;
    public KeyCode jumpKey { get; set; } = KeyCode.Space;
    public KeyCode interactKey { get; set; } = KeyCode.E;

    // Mouse Controls
    public float mouseXSensitivity { get; private set; } = 50f;
    public float mouseYSensitivity { get; private set; } = 50f;
    public float mouseXInput { get; private set; }
    public float mouseYInput { get; private set; }


    void Update()
    {
        GetMouseInput();
        GetKeyboardInput();
    }

    void GetMouseInput() {
        mouseXInput = Input.GetAxisRaw("Mouse X") * mouseXSensitivity * 0.02f;
        mouseYInput = Input.GetAxisRaw("Mouse Y") * mouseYSensitivity * 0.02f;
    }

    void GetKeyboardInput() {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        IsSprinting();
        IsJumping();
        IsCrouching();
    }

    // The below functions have if/else for future support for toggle/hold configuration.
    void IsSprinting() {
        if (Input.GetKey(sprintKey)) {
            SprintPressed = true;
        } else {
            SprintPressed = false;
        }
    }

    void IsJumping() {
        if (Input.GetKeyDown(jumpKey)) {
            JumpPressed = true;
        } else {
            JumpPressed = false;
        }
    }

    void IsCrouching() {
        if (Input.GetKey(crouchKey)) {
            CrouchPressed = true;
        } else {
            CrouchPressed= false;
        }
    }
}
