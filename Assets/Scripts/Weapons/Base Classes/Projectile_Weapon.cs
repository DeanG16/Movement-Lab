using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Weapon : Weapon
{
    [SerializeField]
    protected ProjectileWeaponData weaponData;
    [SerializeField]
    private Transform projectileSpawnPoint;

    private Camera playerCamera;

    private int range;

    #region Projectile Variables
    protected float maximumAmmunition;
    protected float projectileSpread;
    protected float projectileSpeed;
    #endregion

    #region Particles & Effects
    protected ParticleSystem muzzleFlashParticleSystem;
    protected ParticleSystem impactParticleSystem;
    protected TrailRenderer projectileTrailRenderer;
    #endregion

    private float lastShotTime;

    void Awake() {
        this.SetupWeaponData();
        playerCamera = GetComponentInParent<Camera>();
        this.muzzleFlashParticleSystem = this.projectileSpawnPoint.gameObject.GetComponent<ParticleSystem>();
    }

    protected override void PrimaryAction() {
        if (this.lastShotTime + this.primaryActionDelay < Time.time) {
            this.Shoot();
        }
    }

    void Shoot() {
        this.muzzleFlashParticleSystem.Play();
        TrailRenderer projectileTrailRenderer = Instantiate(this.projectileTrailRenderer, this.projectileSpawnPoint.position, Quaternion.identity);
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward * this.range, out RaycastHit hit, float.MaxValue)) {
            StartCoroutine(StartTrailRenderer(projectileTrailRenderer, hit.point, hit.normal, true));
        } else {
            StartCoroutine(StartTrailRenderer(projectileTrailRenderer, playerCamera.transform.forward * this.range, Vector3.zero, false));
        }
        this.lastShotTime = Time.time;
    }

    private IEnumerator StartTrailRenderer(TrailRenderer projectileTrailRenderer, Vector3 hitPoint, Vector3 normal, bool hasCollided) {
        Vector3 startPos = projectileTrailRenderer.transform.position;

        float distance = Vector3.Distance(projectileTrailRenderer.transform.position, hitPoint);
        float startingDistance = distance;


        while (distance > 0) {
            projectileTrailRenderer.transform.position = Vector3.Lerp(startPos, hitPoint, 1 - (distance / startingDistance));
            if (Physics.SphereCast(projectileTrailRenderer.transform.position, 0.02f, projectileTrailRenderer.transform.forward, out RaycastHit impact, 0.02f)) {
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
            projectileTrailRenderer.transform.position = hitPoint;
            ParticleSystem particle = Instantiate(this.impactParticleSystem, hitPoint, Quaternion.LookRotation(normal));
            particle.Play();
        }
        Destroy(projectileTrailRenderer.gameObject, projectileTrailRenderer.time);
    }

    protected void SetupWeaponData() {
        this.name = this.weaponData.name;
        this.damage = this.weaponData.damage;
        this.primaryActionDelay = this.weaponData.primaryActionDelay;
        
        // Projectile Information
        this.maximumAmmunition = this.weaponData.maximumAmmunition;
        this.projectileSpeed = this.weaponData.projectileSpeed;
        this.projectileSpread = this.weaponData.projectileSpreadPercentage;
        this.range = this.weaponData.range;


        // VFX
        this.projectileTrailRenderer = this.weaponData.projectileTrailRenderer;
        this.impactParticleSystem = this.weaponData.impactParticleSystem;

        // Sound FX
        this.primaryActionSoundFx = this.weaponData.primaryActionimpactSoundFx;
        this.impactSoundFx = this.weaponData.primaryActionimpactSoundFx;
    }
}
