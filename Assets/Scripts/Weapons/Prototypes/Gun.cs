using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : ProjectileWeapon
{
/*    Camera cam;
    public Transform fireOrigin;
    private float lastShootTime;

    private void Awake() {
        cam = GetComponentInParent<Camera>();
        transform.rotation = Quaternion.RotateTowards(transform.rotation, cam.transform.rotation, 180f);
    }

    private void Update() {
        if (Input.GetMouseButton(0) && lastShootTime + weaponData.fireDelay < Time.time) {
            lastShootTime = Time.time;
            Shoot();
        }

    }
    public override void Shoot() {
        TrailRenderer trail = Instantiate(weaponData.trailRenderer, fireOrigin.position, Quaternion.identity);
        if (Physics.Raycast(cam.transform.position, cam.transform.forward * weaponData.range, out RaycastHit hit, float.MaxValue)) {
            StartCoroutine(FireBullet(trail, hit.point, hit.normal, true));
        } else {
            StartCoroutine(FireBullet(trail, cam.transform.forward * weaponData.range, Vector3.zero, false));
        }
    }

    private IEnumerator FireBullet(TrailRenderer trail, Vector3 hitPoint, Vector3 normal, bool hasCollided) {
        Vector3 startPos = trail.transform.position;

        float distance = Vector3.Distance(trail.transform.position, hitPoint);
        float startingDistance = distance;


        while (distance > 0) {
            trail.transform.position = Vector3.Lerp(startPos, hitPoint, 1 - (distance/startingDistance));
            if (Physics.SphereCast(trail.transform.position, 0.05f, trail.transform.forward, out RaycastHit impact, 0.05f)) {
                distance = 0;
                hitPoint = impact.point;
                normal = impact.normal;
                hasCollided = true;
            } else {
                distance -= Time.deltaTime * weaponData.projectileSpeed;
            }
            yield return null;
        }
        if (hasCollided) {
            trail.transform.position = hitPoint;
            ParticleSystem particle = Instantiate(weaponData.impactParticle, hitPoint, Quaternion.LookRotation(normal));
            particle.Play();
        }
        Destroy(trail.gameObject, trail.time);
    }*/
}
