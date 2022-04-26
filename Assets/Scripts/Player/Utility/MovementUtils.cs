using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementUtils
{
    public static Vector2 LookDirectionVelocity(Vector3 rbVelocity, float yRotation) {
        // Get angle between current orientation (Y axis) and rigidbody's velocity direction, converting to degrees.
        // This is so we limit movement in the correct direction and not the global rotation of the player.
        float velFromOrientationAngle = Mathf.DeltaAngle(yRotation, Mathf.Atan2(rbVelocity.x, rbVelocity.z) * Mathf.Rad2Deg);
        float magnitude = new Vector2(rbVelocity.x, rbVelocity.z).magnitude;

        // Calculate X and Z magnitude, converting degrees back to radians and account for Unity's clockwise Y rotation.
        // > X -> Sin(Theta) = Cos(90 - Angle).
        // > Z -> Cos(Theta) = Sin(90 - Angle).
        float xMag = magnitude * Mathf.Sin(velFromOrientationAngle * Mathf.Deg2Rad);
        float zMag = magnitude * Mathf.Cos(velFromOrientationAngle * Mathf.Deg2Rad);

        return new Vector2(xMag, zMag);
    }
}
