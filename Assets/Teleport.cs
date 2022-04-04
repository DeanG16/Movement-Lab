using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform teleportLocation;

    private void OnCollisionEnter(Collision collision) {
        collision.gameObject.transform.position = teleportLocation.position;
    }
}
