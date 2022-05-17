using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST_Weapon : MonoBehaviour
{
    public GameObject projectile;
    public Transform firingOrigin;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0)) {
            Fire();
        }
    }

    void Fire() {
        GameObject bullet = Instantiate(projectile, firingOrigin.position, Quaternion.LookRotation(transform.forward));
    }
}
