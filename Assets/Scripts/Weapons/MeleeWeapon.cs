﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    //public float meleeRange;
    //how fast can we attack?
   // public float meleeAttackInterval;
    float nextPrepareMeleeAttackTime;

    public Animator weaponAnimator;

    float nextMeleeAttackTime;
    //how long does it take for the swing to hit its target?
    //public float attackDuration;
    bool attack;

    public bool drawDamageGizmo;
    /*[Tooltip("position relative to the unit")]
    public Vector3 hitPosition;
    public float hitSphereRadius;

    [Header("pushing")]
    public bool pushes;
    public float pushForce;*/

    //TODO only like this for now, change this later to be dependant also on other things
    int attackID = 1;

    public MeleeAttack[] attacks;
    MeleeAttack currentAttack;

    private void Start()
    {
        currentAttack = attacks[attackID];
    }


    public override void HandleLMBDown()
    {
        if (Time.time > nextPrepareMeleeAttackTime)
        {


            Attack(attackID);
            Debug.Log("attack: " + attackID);
            if (attackID == 0)
            {
                attackID = 1;
            }
            else
            {
                attackID = 0;
            }
        }
        else Debug.Log("attack: too fast" );

    }

    public override void HandleLMBHold()
    {
    }

    public override void OnWeaponSelect()
    {
        nextPrepareMeleeAttackTime = 0;
        nextMeleeAttackTime = 0;
        attack = false;
    }

    public void Attack(int attackID)
    {
        currentAttack = attacks[attackID];      
            //attack
     
            nextPrepareMeleeAttackTime = Time.time + currentAttack.meleeAttackInterval;

            //target.TakeDamage(meleeDamage);
            attack = true;
            nextMeleeAttackTime = Time.time + currentAttack.attackDuration;
            weaponAnimator.SetTrigger(currentAttack.animationName);
            //currentTarget = target;
    }

    private void Update()
    {
        if (attack)
        {
            if (Time.time > nextMeleeAttackTime)
            {
                ExecuteAttack();
                attack = false;
            }
        }
    }

    void ExecuteAttack()
    {
        // if (currentTarget != null) currentTarget.TakeDamage(meleeDamage);

        Collider[] visibleColliders = Physics.OverlapSphere(transform.TransformPoint(currentAttack.hitPosition), currentAttack.hitSphereRadius);

        for (int i = 0; i < visibleColliders.Length; i++)
        {
            IDamageable<float> damageable = visibleColliders[i].gameObject.GetComponent<IDamageable<float>>();

            // Debug.Log("collider " + visibleColliders[i]);
            if (damageable != null)
            {
                // check who did we hit, check if he has an gameEntity
                GameEntity entity = visibleColliders[i].gameObject.GetComponent<GameEntity>();
                // Debug.Log("damegable entity: " + entity);
                if (entity != null)
                {
                    if (!Settings.Instance.friendlyFire)
                    {
                        DiplomacyStatus diplomacyStatus = Settings.Instance.GetDiplomacyStatus(teamID, entity.teamID);
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
                            direction = (visibleColliders[i].gameObject.transform.position - transform.position).normalized;
                        }
                        else
                        {
                            direction = currentAttack.pushDirection;
                        }
                        
                        // Debug.Log("push: " + pusheable);
                        pusheable.Push(direction * currentAttack.pushForce);
                    }
                }


                return;
            }

        }




    }

    void GiveDamage(IDamageable<float> damageable, GameObject enemyTransform)
    {
        if (currentAttack.pushes)
        {
            if (currentAttack.defaultDirection)
            {
                damageable.TakeDamage(currentAttack.damage, (enemyTransform.transform.position - transform.position).normalized * currentAttack.pushKillForce);
            }
            else
            {
                damageable.TakeDamage(currentAttack.damage, transform.TransformDirection(currentAttack.pushDirection) * currentAttack.pushKillForce);
            }         
        }
        else
        {
            damageable.TakeDamage(damage);
        }
    }

    public bool CanAttack()
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
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.TransformPoint(currentAttack.hitPosition), currentAttack.hitSphereRadius);
            }
           
        }
    }
}

