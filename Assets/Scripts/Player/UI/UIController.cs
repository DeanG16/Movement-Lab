using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Max Speed Value")]
    public Text maxSpeedValue;

    [Header("Speed Value")]
    public Text speedValue;

    [Header("Angle Value")]
    public Text slopeAngleValue;

    [Header("Jumping Value")]
    public Text jumpingValue;

    [Header("Slope Value")]
    public Text slopeValue;

    [Header("Grounded Value")]
    public Text groundedValue;

    [Header("Airborne Value")]
    public Text airborneValue;

    [Header("Sliding Value")]
    public Text slidingValue;

    [Header("Exiting Slide Value")]
    public Text exitingSlideValue;

    // Start is called before the first frame update
    void Start()
    {
        PlayerMovement playerMove = FindObjectOfType<PlayerMovement>();
        StateManager stateManager = FindObjectOfType<StateManager>();
        playerMove.magnitudeChange += UpdateMagnitude;
        stateManager.slopeAngleChange += UpdateSlopeAngle;
        stateManager.slidingChange += UpdateSliding;
        stateManager.groundedChange += UpdateGrounded;
        stateManager.slopeChange += UpdateSlope;
        stateManager.exitingSlideChange += UpdateExitingSlide;
        playerMove.maxSpeedChange += UpdateMaxSpeed;
    }

    void UpdateMagnitude(float value) {
        speedValue.text = (Mathf.Floor(value * 10.0f) * 0.1f).ToString();
    }

    void UpdateMaxSpeed(float value) {
        maxSpeedValue.text = value.ToString();
    }

    void UpdateExitingSlide(bool value) {
        exitingSlideValue.color = !value ? Color.red : Color.green;
        exitingSlideValue.text = value.ToString();
    }

    void UpdateSlopeAngle(float value) {
        if(value > 50f) {
            slopeAngleValue.color = Color.red;
        } else {
            slopeAngleValue.color = Color.green;
        }
        slopeAngleValue.text = (Mathf.Floor(value * 10.0f) * 0.1f).ToString();
    }

    void UpdateGrounded(bool value) {
        groundedValue.color = !value ? Color.red : Color.green;
        groundedValue.text = value.ToString();
    }

    void UpdateSliding(bool value) {
        slidingValue.color = !value ? Color.red : Color.green;
        slidingValue.text = value.ToString();
    }

    void UpdateSlope(bool value) {
        slopeValue.color = !value ? Color.red : Color.green;
        slopeValue.text = value.ToString();
    }
}
