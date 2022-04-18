using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform teleportLocation;
    public float delay = 0f;
    private GameObject teleportObject;

    void OnTriggerEnter(Collider collision) {
        teleportObject = collision.gameObject;
        if (delay == 0f) {
            TeleportToTarget();
        } else {
            Invoke("TeleportToTarget", delay);
        }
    }

    private void TeleportToTarget() {
        teleportObject.transform.position = teleportLocation.position;
    }
}
