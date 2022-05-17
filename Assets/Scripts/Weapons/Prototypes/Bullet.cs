using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    private ParticleSystem impact;

    private bool hit;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        impact = GetComponentInChildren<ParticleSystem>();
        rb.AddForce(transform.forward * 6000f * Time.deltaTime, ForceMode.Impulse);
    }


    private void Update() {
        if(!hit) {
            
        }
    }

    private void OnCollisionEnter(Collision other) {
        hit = true;
        rb.velocity = Vector3.zero;
        impact.transform.position = other.contacts[0].point;
        impact.transform.rotation = Quaternion.LookRotation(-transform.forward);
        impact.Play();

        Invoke("DestroySelf", 0.25f);
    }

    void DestroySelf() {
        Destroy(gameObject);
    }
}
