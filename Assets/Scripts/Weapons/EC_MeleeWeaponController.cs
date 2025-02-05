﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Animates the 2 Hnds and the spine and controls the different missile and melee Weapons
//later he also takes care of the change weapon animation - between the changing the current weapon is null for a few seconds

//refactor takes a bit from the human weaponSystem


//weapon system takes care of changing and reloading weapon and having an inventory, can also be used by units. WeaponController is used to use the currentSelectedWeapon, weaponController should be able to work without weaponSystem

public class EC_MeleeWeaponController : EntityComponent
{
    //if you use a unit which has spine movement, attack this script to the spine
    public Weapon currentWeapon;
    [Tooltip("the hitboxes of the attacks are relative to this transform - usefull with spine")]
    public Transform relativeTransform;

    /* public enum WeaponControllerState 
     {
         NoWeapon,
         MissileWeapon,
         MeleeWeapon
     }

     public WeaponControllerState state;*/

    /*[Header("Missile")]

    [Tooltip("how fast do we aim")]
    public float turningSpeed;

    //public Transform parent;
    bool aiming = false;
    GameEntity currentTarget;

    public bool gravityProjectile;
    //projectuile needs to weight 1kg?
    bool currentTargetHasMovement; //does currentTarget has a movementController
    EC_Movement currentEnemyMovement;

    [Tooltip("the bullet gets rotated randomly upon shooting, so we add some skill based aiming")]
    public float shootingError;

    public bool reloading = false;
    float reloadEndTime;*/


    [Header("Melee")]

    public SO_MeleeAttackSet[] attackSets; //if we have different AttackSets, then we change it somehow
    public int currentAttackSet;
    SO_MeleeAttack currentAttack;
    int attackID = 1;


    float nextPrepareMeleeAttackTime;     //how fast can we attack?
    float nextMeleeAttackExecuteTime;     //how long does it take for the swing to hit its target?
    public bool meleeAttackInitiated;  //are we currently attacking?

    [Header("Animation")]
    public Animator handsAnimator;
    //TODO public SpineAimer spineAimier //a simple small script, which aims the spine to the current height- cnot necessary should work with or without it

    [Tooltip("needs to be atleast the length of the attack_end animation clip")]
    public float meleeAttackInterval; //time between attacks, move it elsewhere later into unit behaviour or something similar?


    public bool drawDamageGizmo;

    //sound
    float playSwingSoundTime;
    bool playSwingSoundDelayed = false;



    public override void SetUpComponent(GameEntity entity)
    {
        base.SetUpComponent(entity);
        if (attackSets[0] == null) Debug.Log("attacksets is null");
        if (attackSets[0].attacks[0] == null) Debug.Log("attackSets[0] == null");
        /*foreach(MeleeAttackSet attack in attackSets)
        {
            Debug.Log("--------attack set: -" + attack);
            //foreach(MeleeAttack atta in attack.attacks)
            //{
               // Debug.Log("attack: " + atta);
            //}
        }*/

        //SetWeapon(currentWeapon);
    }

    public override void UpdateComponent()
    {
        if (meleeAttackInitiated)
        {
            if (Time.time > nextMeleeAttackExecuteTime)
            {
                ExecuteMeleeAttack();
                meleeAttackInitiated = false;
            }
            if (playSwingSoundDelayed)
            {
                if (Time.time > playSwingSoundTime)
                {
                    playSwingSoundDelayed = false;

                    //(currentWeapon as MeleeWeapon).audioSource.Stop();
                    (currentWeapon as MeleeWeapon).audioSource.clip = currentAttack.swingSound;
                    (currentWeapon as MeleeWeapon).audioSource.Play();
                }
            }
        }
    }

    //sets the current weapon and the corresponding attack set
    public void SetWeapon(MeleeWeapon newWeapon)
    {
        currentWeapon = newWeapon;
        currentAttackSet = newWeapon.attackSetID;
        meleeAttackInitiated = false;
    }

    /*public void SetWeapon(Weapon newWeapon)
    {
        if(newWeapon is MeleeWeapon)
        {
            state = WeaponControllerState.MeleeWeapon;
        }
        else if(newWeapon is MissileWeapon)
        {
            state = WeaponControllerState.MissileWeapon;
        }
        else
        {
            state = WeaponControllerState.NoWeapon;
        }


        currentWeapon = newWeapon;
    }*/

    public void MeleeAttack()
    {
        if (Time.time > nextPrepareMeleeAttackTime)
        {
            Attack(attackID);
            //Debug.Log("attack: " + attackID);
            attackID = Random.Range(0, attackSets[currentAttackSet].attacks.Length);
        }
    }

    public void Attack(int attackID)
    {
        currentAttack = attackSets[currentAttackSet].attacks[attackID];


        //nextPrepareMeleeAttackTime = Time.time + currentAttack.meleeAttackInterval;
        playSwingSoundTime = Time.time + currentAttack.swingSoundDelay;
        playSwingSoundDelayed = true;
        nextPrepareMeleeAttackTime = Time.time + currentAttack.attackDuration + meleeAttackInterval;

        //target.TakeDamage(meleeDamage);
        meleeAttackInitiated = true;
        nextMeleeAttackExecuteTime = Time.time + currentAttack.attackDuration;
        //Debug.Log("set trigger: " + currentAttack.animationName);

        handsAnimator.SetFloat("MeleeWeaponSpeed" + currentWeapon.stanceAnimationTypeID, 1 / currentAttack.attackDuration);
        handsAnimator.SetTrigger(currentAttack.animationName);
        
        //currentTarget = target;


    }

    void ExecuteMeleeAttack()
    {
       // Debug.Log("Execute");
        // if (currentTarget != null) currentTarget.TakeDamage(meleeDamage);

        Collider[] visibleColliders = Physics.OverlapSphere(relativeTransform.TransformPoint(currentAttack.hitPosition), currentAttack.hitSphereRadius);
        if (visibleColliders.Length > 0)
        {
            (currentWeapon as MeleeWeapon).audioSource.Stop();
            (currentWeapon as MeleeWeapon).audioSource.clip = currentAttack.hitSound;
            (currentWeapon as MeleeWeapon).audioSource.Play();
        }
        //delete player from visible collliders
       
        for (int i = 0; i < visibleColliders.Length; i++)
        {
            //Debug.Log("collides with: " + visibleColliders[i]);
            if (visibleColliders[i].gameObject != myEntity.gameObject) //so we dont hit ourselves, maybe change this to a better solution in the future
            {
                IDamageable<DamageInfo> damageable = visibleColliders[i].gameObject.GetComponent<IDamageable<DamageInfo>>();

                // Debug.Log("collider " + visibleColliders[i]);
                if (damageable != null)
                {
                    // check who did we hit, check if he has an gameEntity
                    GameEntity entity = visibleColliders[i].gameObject.GetComponent<GameEntity>();
                    // Debug.Log("damegable entity: " + entity);
                    if (entity != null)
                    {
                        if (!SceneSettings.Instance.friendlyFire)
                        {
                            DiplomacyStatus diplomacyStatus = SceneSettings.Instance.GetDiplomacyStatus(currentWeapon.teamID, entity.teamID);
                            if (diplomacyStatus == DiplomacyStatus.War)
                            {
                                GiveDamage(damageable, visibleColliders[i].gameObject);
                            }

                        }
                        else
                        {
                            GiveDamage(damageable, visibleColliders[i].gameObject);
                        }

                    }
                    else
                    {
                        GiveDamage(damageable, visibleColliders[i].gameObject);
                    }

                    if (currentAttack.pushes)
                    {
                        IPusheable<Vector3> pusheable = visibleColliders[i].gameObject.GetComponent<IPusheable<Vector3>>();

                        if (pusheable != null)
                        {
                            Vector3 direction;
                            if (currentAttack.defaultDirection)
                            {
                                if (entity != null)
                                {
                                    direction = (entity.transform.position + entity.aimingCorrector - relativeTransform.position).normalized;
                                }
                                else
                                {
                                    direction = (visibleColliders[i].gameObject.transform.position - relativeTransform.position).normalized;
                                }
                            }
                            else
                            {
                                direction = relativeTransform.TransformDirection(currentAttack.pushDirection.normalized);
                            }

                            pusheable.Push(direction * currentAttack.pushForce * SceneSettings.Instance.forceMultiplier);
                        }
                    }


                    return;
                }
            }

        }
    }

    public void AbortMeleeAttack()
    {
        meleeAttackInitiated = false;
        handsAnimator.SetTrigger("AbortMeleeAttack");
    }

    void GiveDamage(IDamageable<DamageInfo> damageable, GameObject enemyTransform)
    {
        //we check for pushes so the final hit can push him back
        if (currentAttack.pushes)
        {
            if (currentAttack.defaultDirection)
            {
                //the push force here will only be applied to the corpse
                damageable.TakeDamage(new DamageInfo(currentAttack.damage, (enemyTransform.transform.position - transform.position).normalized * currentAttack.pushKillForce, myEntity));
            }
            else
            {
                damageable.TakeDamage(new DamageInfo(currentAttack.damage, transform.TransformDirection(currentAttack.pushDirection) * currentAttack.pushKillForce, myEntity));
            }
        }
        else
        {
            damageable.TakeDamage(new DamageInfo(currentWeapon.damage));
        }
    }

    public bool CanMeleeAttack()
    {
        if (Time.time > nextPrepareMeleeAttackTime) return true;
        else return false;
    }

    private void OnDrawGizmos()
    {

        if (drawDamageGizmo)
        {
            if (currentAttack != null)
            {
                //Debug.Log("current attack not null");
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(relativeTransform.TransformPoint(currentAttack.hitPosition), currentAttack.hitSphereRadius);
            }
        }
    }
}
