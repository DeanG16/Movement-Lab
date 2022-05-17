using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee_Weapon : Weapon
{
    protected override void PrimaryAction() {
        Debug.Log("Melee Primary action");
    }
}
