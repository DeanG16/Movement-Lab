using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Rigidbody Magnitude Value")]
    public Text rbMagValue;

    [Header("Slope Angle Value")]
    public Text slopeAngleValue;

    // Start is called before the first frame update
    void Start()
    {
        PlayerMovement playerMove = FindObjectOfType<PlayerMovement>();
        playerMove.magnitudeChange += UpdateMagnitude;
        playerMove.slopeAngleChange += UpdateSlopeAngle;
    }

    void UpdateMagnitude(float value) {
        rbMagValue.text = (Mathf.Floor(value * 10.0f) * 0.1f).ToString();
    }

    void UpdateSlopeAngle(float value) {
        if(value > 45f) {
            slopeAngleValue.color = Color.red;
        } else {
            slopeAngleValue.color = Color.green;
        }
        slopeAngleValue.text = (Mathf.Floor(value * 10.0f) * 0.1f).ToString();
    }
}
