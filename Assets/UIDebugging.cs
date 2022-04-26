using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDebugging : MonoBehaviour
{
    private StateHandler stateHandler;

    #region Physics
    public Text grounded;
    public Text slope;
    public Text sliding;
    #endregion

    #region Movement
    public Text sprinting;
    public Text crouching;
    public Text jumping;
    #endregion

    private void Awake() {
        stateHandler = GetComponentInParent<StateHandler>();
    }

    void Update()
    {
        grounded.color = stateHandler.IsGrounded ? Color.green : Color.red;
        slope.color = stateHandler.IsOnSlope ? Color.green : Color.red;
        sliding.color = stateHandler.IsSliding ? Color.green : Color.red;

        sprinting.color = stateHandler.IsSprinting ? Color.green : Color.red;
        crouching.color = stateHandler.IsCrouching ? Color.green : Color.red;
        jumping.color = stateHandler.IsJumping ? Color.green : Color.red;
    }
}
