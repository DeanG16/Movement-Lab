using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositioner : MonoBehaviour
{
    [Header("Target Transform")]
    public Transform targetPosition;

    void Update() {
        if (targetPosition == null) {
            Debug.LogError("CameraPositioner.cs: target assigned");
            return;
        }

        transform.position = targetPosition.position;
    }
}
