﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New MeleeAttackSet", menuName = "MeleeAttackSetSO")]
public class MeleeAttackSet : ScriptableObject
{
    public string setName;
    public MeleeAttack[] attacks;
}


//some meleeWeapons have 
[CreateAssetMenu(fileName = "New MeleeAttack", menuName = "MeleeAttackSO")]
public class MeleeAttack : ScriptableObject
{
    [Header("Attack")]
    public string attackName;
    public float damage;
    public float meleeAttackInterval;

    //how long does it take for the swing to hit its target?
    public float attackDuration;

    // public bool drawDamageGizmo;
    [Tooltip("position relative to the unit")]
    public Vector3 hitPosition;
    public float hitSphereRadius;

    [Header("pushing")]
    public bool pushes;
    public float pushForce;
    public float pushKillForce;
    public Vector3 pushDirection;
    public bool defaultDirection = true; //if this is true we just push the enemy away from us

    public string animationName;

}
