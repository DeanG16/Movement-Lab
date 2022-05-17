using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Weapon/Projectile Weapon")]
public class ProjectileWeaponData : WeaponDataObject
{
    public float projectileSpeed;
    public bool hasProjectileSpread;
    public int range;
    [Range(0f, 10f)]
    public float projectileSpreadPercentage;
    public int maximumAmmunition;
    public TrailRenderer projectileTrailRenderer;
}
