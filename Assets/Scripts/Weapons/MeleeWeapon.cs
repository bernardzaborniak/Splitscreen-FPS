﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("-----------------Melee Weapon---------------")]
    [Tooltip("which set of attacks is possible with this weapon? - communicates with meleeWeaponsController")]
    public int attackSetID;

}

