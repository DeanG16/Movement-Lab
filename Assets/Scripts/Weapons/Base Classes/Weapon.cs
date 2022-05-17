using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected new string name;
    protected float damage;
    protected float primaryActionDelay;
    protected AudioClip[] primaryActionSoundFx;
    protected AudioClip[] impactSoundFx;

    void Update() {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) {
            this.PrimaryAction();
        }
    }

    protected abstract void PrimaryAction();
}
