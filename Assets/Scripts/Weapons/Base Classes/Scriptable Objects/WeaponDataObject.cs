using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDataObject : ScriptableObject
{
    public new string name;
    public float damage;
    public float primaryActionDelay;
    public AudioClip[] primaryActionSoundFx;
    public AudioClip[] primaryActionimpactSoundFx;
    public ParticleSystem impactParticleSystem;
}
