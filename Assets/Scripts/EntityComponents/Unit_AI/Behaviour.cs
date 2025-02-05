﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Behaviour 
{
    public float behaviourUpdateInterval;
    float nextBehaviourUpdateTime;
    [HideInInspector]
    //every behaviour needs some components that the unitAI needs to have saved
    public GameEntity entity;

    /*public Behaviour(Ab_UnitAI unitAI)
    {
        this.unitAI = unitAI;
    }*/

    public virtual void SetUpBehaviour(GameEntity entity)
    {
        this.entity = entity;
    }

    public virtual void UpdateBehaviour()
    {
        if(Time.time > nextBehaviourUpdateTime)
        {
            nextBehaviourUpdateTime = Time.time + behaviourUpdateInterval;
            Update();
        }
    }

    protected virtual void Update()
    {

    }

    public virtual void OnBehaviourEnter()
    {

    }

    public virtual void OnBehaviourExit()
    {

    }

    public virtual void OnDie()
    {

    }
}

[System.Serializable]
public class B_WanderAroundPosition: Behaviour
{
    public float maxDistanceToPositionWhileWandering;
    EC_Movement movement;
    public Transform positionToWanderAround;

    public void SetUpBehaviour(GameEntity entity, EC_Movement movement)
    {
        this.entity = entity;
        this.movement = movement;
        if (positionToWanderAround == null)
        {
            positionToWanderAround = entity.transform;
        }
    }

    public void SetPositionToWanderAround(Transform newPositionToWanderAround)
    {
        positionToWanderAround = newPositionToWanderAround;
    }

    protected override void Update()
    {

            Vector3 wanderPoint = UnityEngine.Random.insideUnitSphere * 4;
            wanderPoint.y = entity.transform.position.y;

            wanderPoint += entity.transform.forward * 4 + entity.transform.position;

            //if he would stray off to far, bring him back to base
            if (Vector3.Distance(positionToWanderAround.position, wanderPoint) > maxDistanceToPositionWhileWandering)
            {
                wanderPoint += (positionToWanderAround.position - wanderPoint) / 4;
            }

            movement.MoveTo(wanderPoint);
        
    }
}

[System.Serializable]
public class B_Flee: Behaviour
{
    EC_Sensing sensing;
    EC_Movement movement;

    public void SetUpBehaviour(GameEntity entity, EC_Movement movement, EC_Sensing sensing)
    {
        this.entity = entity;
        this.movement = movement;
        this.sensing = sensing;
    }

    protected override void Update()
    {
        if (sensing.nearestEnemy != null)
        {
            movement.MoveTo((entity.transform.position - sensing.nearestEnemy.transform.position).normalized*5);
        }
        else
        {
            movement.Stop();
        }
    }


}

[System.Serializable]
public class B_MeleeFighter : Behaviour
{
    EC_Sensing enemySensing;
    EC_Movement movement;
    Animator handsAnimator;

    public float distanceCheckingInterval;
    float nextDistanceCheckTime;

    //the fighter will be between this distances, he does not need to stop to attack
    public float perfectMeleeDistance;
    public float maxMeleeDistance;
    //public float minMeleeDistance;

    float myWidth;
    float enemyWidth;

    //meleefighting
    //MeleeWeapon weapon;
    EC_MeleeWeaponController weaponController;
    [SerializeField]
    bool inRange;

    public void SetUpBehaviour(GameEntity entity, EC_Movement movement, EC_Sensing enemySensing, EC_MeleeWeaponController weaponController)//, MeleeWeapon weapon)
    {
        this.entity = entity;
        this.movement = movement;
        this.enemySensing = enemySensing;
        //this.weapon = weapon;
        this.weaponController = weaponController;

        nextDistanceCheckTime = UnityEngine.Random.Range(0, distanceCheckingInterval);
        maxMeleeDistance *= maxMeleeDistance;
    }

    public void SetUpBehaviour(GameEntity entity, EC_Movement movement, EC_Sensing enemySensing, EC_MeleeWeaponController weaponController, Animator handsAnimator)//, MeleeWeapon weapon)
    {
        this.entity = entity;
        this.movement = movement;
        this.enemySensing = enemySensing;
        //this.weapon = weapon;
        this.weaponController = weaponController;
        this.handsAnimator = handsAnimator;

        nextDistanceCheckTime = UnityEngine.Random.Range(0, distanceCheckingInterval);
        maxMeleeDistance *= maxMeleeDistance;
    }


    protected override void Update()
    {
        //Debug.Log("unit Update");
        movement.LookAt(enemySensing.nearestEnemy.GetPositionForAiming()- entity.GetPositionForAiming());

        if (Time.time > nextDistanceCheckTime)
        {
            nextDistanceCheckTime += distanceCheckingInterval;

            myWidth = entity.width;
            enemyWidth = enemySensing.nearestEnemy.width;

            Vector3 nearestEnemyPosition = enemySensing.nearestEnemy.transform.position;
            Vector3 myPosition = entity.transform.position;

            float widthFactor = myWidth + enemyWidth; //multiply the resulting distanceVectorBythisFactor to also use width
            Vector3 distanceVector = nearestEnemyPosition - myPosition;
            //float distanceToEnemySquared = (distanceVector - distanceVector.normalized * widthFactor).sqrMagnitude;
            float distanceToEnemy = (distanceVector - distanceVector.normalized * widthFactor).magnitude;

            //if the enemy is moving, we move to the position he will be at the time we arrive
            EC_Movement enemyMovement = enemySensing.nearestEnemy.GetComponent<EC_Movement>();

            

            if (enemyMovement.IsMoving())
            {
                    //heuristically calculae future position
                    //1. how long will it take for me to reach the enemy?
                    float timeToReachEnemy = distanceToEnemy / movement.GetMaxSpeed();
                    //2. where will the enemy be after this time
                    Vector3 futurePosition = nearestEnemyPosition + enemyMovement.GetCurrentVelocity() * timeToReachEnemy;

                //if the future positionof the target is behind the follower, he should not run backward

                if (Vector3.Angle((futurePosition - myPosition), (nearestEnemyPosition - myPosition)) > 90)
                {
                    movement.MoveTo(nearestEnemyPosition + (myPosition - nearestEnemyPosition).normalized * (perfectMeleeDistance + myWidth + enemyWidth));
                }
                else
                {
                    movement.MoveTo(futurePosition);
                }


            }   
            else
            {
                movement.MoveTo(nearestEnemyPosition + (myPosition - nearestEnemyPosition).normalized * (perfectMeleeDistance+myWidth+enemyWidth));
            }

            if ((nearestEnemyPosition - myPosition).sqrMagnitude > maxMeleeDistance)
            {
                inRange = false;
                // movement.StopLookAt();
                //Debug.Log("notInRange");
            }
            else
            {
                inRange = true;
                // movement.LookAt(enemySensing.nearestEnemy.transform);

            }
        }

        //Debug.Log("in range? : " + inRange);

        if (inRange)
        {
            /*if (weaponController.CanMeleeAttack())
            {
                weaponController.HandleWeaponKeyHold(0);
            }*/
            weaponController.MeleeAttack();
        }

    }

    public override void OnBehaviourEnter()
    {
        if (handsAnimator != null)
        {
            //handsAnimator.SetBool("CombatStance1", true);
            handsAnimator.SetBool("IdleStance", false);
        }
        movement.ActivateLookAt();
    }

    public override void OnBehaviourExit()
    {     
        movement.Stop();
        movement.StopLookAt();
        inRange = false;

        if (handsAnimator != null)
        {
            //handsAnimator.SetBool("CombatStance1", false);
        }

        /*if (handsAnimator != null)
        {
            handsAnimator.SetTrigger("EnterIdleStanceMelee1");
        }*/
    }
}

[System.Serializable]
public class B_MissileFighter : Behaviour
{
    //refactor and add distance checking with width

    EC_Movement movement;
    EC_Sensing enemySensing;
    EC_MissileWeaponController weaponController;
    Animator handsAnimator;
    EC_HumanWeaponSystem weaponSystem;
    //goes to nearest enemy, shoots  and looks at them when in range, tries not to get too close

    public float perfectShootingDistance;
    public float maxShootingDistance;
    bool inRange;

    float myWidth;
    float enemyWidth;


    public float distanceCheckingInterval;
    float nextDistanceCheckTime;

    [Tooltip("to make the bahaviour more realistical, the unit starts shooting after a small delay after entering this behaviour")]
    public float shootingStartDelay;
    float shootingStartTime;
    bool shootingStarted;

    public void SetUpBehaviour(GameEntity entity, EC_Movement movement, EC_Sensing enemySensing, EC_MissileWeaponController weapon)
    {
        this.enemySensing = enemySensing;
        this.entity = entity;
        this.movement = movement;
        this.weaponController = weapon;

        nextDistanceCheckTime = UnityEngine.Random.Range(0, distanceCheckingInterval);

        maxShootingDistance *= maxShootingDistance;
    }

    public void SetUpBehaviour(GameEntity entity, EC_Movement movement, EC_Sensing enemySensing, EC_MissileWeaponController weapon, Animator handsAnimator, EC_HumanWeaponSystem weaponSystem)
    {
        this.enemySensing = enemySensing;
        this.entity = entity;
        this.movement = movement;
        this.weaponController = weapon;
        this.handsAnimator = handsAnimator;
        this.weaponSystem = weaponSystem;

        nextDistanceCheckTime = UnityEngine.Random.Range(0, distanceCheckingInterval);

        maxShootingDistance *= maxShootingDistance;
    }

    protected override void Update()
    {
       // Debug.Log("updateAI");
        if (Time.time > nextDistanceCheckTime)
        {
            nextDistanceCheckTime += distanceCheckingInterval;

            myWidth = entity.width;
            enemyWidth = enemySensing.nearestEnemy.width;

            Vector3 nearestEnemyPosition = enemySensing.nearestEnemy.transform.position;
            Vector3 myPosition = entity.transform.position;

            float widthFactor = myWidth + enemyWidth; //multiply the resulting distanceVectorBythisFactor to also use width
            Vector3 distanceVector = nearestEnemyPosition - myPosition;
            float distanceToEnemy = (distanceVector - distanceVector.normalized * widthFactor).sqrMagnitude;

            movement.MoveTo(nearestEnemyPosition + (myPosition - nearestEnemyPosition).normalized * (perfectShootingDistance + myWidth + enemyWidth));
     
            if ((nearestEnemyPosition - myPosition).sqrMagnitude > maxShootingDistance)
            {
                inRange = false;
                //movement.StopLookAt();
                weaponController.StopAiming();
                //Debug.Log("stop aim");

               // movement.StopAimAt();

            }
            else
            {
                inRange = true;
                // Debug.Log("start aim");
                //movement.LookAt(enemySensing.nearestEnemy.transform);
                weaponController.AimAt(enemySensing.nearestEnemy);

                //movement.AimAt(enemySensing.nearestEnemy, weapon.transform);

            }
        }

        if (!shootingStarted)
        {
            if (Time.time > shootingStartTime) shootingStarted = true;
        }
        else
        {
            if (inRange)
            {
                //Debug.Log("weaponController");
                if (weaponController.HasEnoughAmmoLoaded())
                {
                    if (weaponSystem.CanShoot())
                    {

                         weaponController.Shoot();


                        //Debug.Log("shot on state: " + weaponSystem.state);
                    }

                }
                else if (!weaponSystem.IsReloading())
                {
                    //Debug.Log("not enough ammo");
                    //weaponSystem.StartReloading();
                    //Debug.Log("reload");
                }
            }
        }

       

       
    }

    public override void OnBehaviourExit()
    {
        /*if (handsAnimator != null)
        {
            handsAnimator.SetTrigger("EnterIdleStanceMissile1");
        }*/
        movement.Stop();
        //movement.StopLookAt();
        weaponController.StopAiming();

        inRange = false;

        if (handsAnimator != null)
        {
           // handsAnimator.SetBool("CombatStance2", false);

        }
    }

    public override void OnBehaviourEnter()
    {
        shootingStarted = false;
        shootingStartTime = Time.time + shootingStartDelay;

        if (handsAnimator != null)
        {
            //handsAnimator.SetBool("CombatStance2", true);
            handsAnimator.SetBool("IdleStance", false);

        }
    }
}

[System.Serializable]
public class B_HumanoidIdle : Behaviour
{
    Animator handsAnimator;

    public void SetUpBehaviour( Animator handsAnimator)
    {
        this.handsAnimator = handsAnimator;
    }


    public override void OnBehaviourExit()
    {
        if (handsAnimator != null)
        {
           // handsAnimator.SetBool("IdleStance", false);
        }
    }

    public override void OnBehaviourEnter()
    {
        if (handsAnimator != null)
        {
            handsAnimator.SetBool("IdleStance", true);

           // Debug.Log("enter idle stance");
        }
    }
}

[System.Serializable]
public class B_ElectrocuterDrone : Behaviour
{
    EC_Sensing enemySensing;
    EC_Movement movement;
    // ElectrocutterEffect - use it onAttack

    public float distanceCheckingInterval;
    float nextDistanceCheckTime;

    //the fighter will be between this distances, he does not need to stop to attack
    public float perfectMeleeDistance;
    public float maxMeleeDistance;

    float myWidth;
    float enemyWidth;


   // EC_ElectroField
    [SerializeField]
    bool inRange;

    public void SetUpBehaviour(GameEntity entity, EC_Movement movement, EC_Sensing enemySensing)//, EC_ElectroField )
    {
        this.entity = entity;
        this.movement = movement;
        this.enemySensing = enemySensing;

        nextDistanceCheckTime = UnityEngine.Random.Range(0, distanceCheckingInterval);
        maxMeleeDistance *= maxMeleeDistance;
    }


    protected override void Update()
    {
        movement.LookAt(enemySensing.nearestEnemy.GetPositionForAiming() - entity.GetPositionForAiming());

        if (Time.time > nextDistanceCheckTime)
        {
            nextDistanceCheckTime += distanceCheckingInterval;

            myWidth = entity.width;
            enemyWidth = enemySensing.nearestEnemy.width;

            Vector3 nearestEnemyPosition = enemySensing.nearestEnemy.transform.position;
            Vector3 myPosition = entity.transform.position;

            float widthFactor = myWidth + enemyWidth; //multiply the resulting distanceVectorBythisFactor to also use width
            Vector3 distanceVector = nearestEnemyPosition - myPosition;
            float distanceToEnemy = (distanceVector - distanceVector.normalized * widthFactor).magnitude;

            //if the enemy is moving, we move to the position he will be at the time we arrive
            EC_Movement enemyMovement = enemySensing.nearestEnemy.GetComponent<EC_Movement>();



            if (enemyMovement.IsMoving())
            {
                //heuristically calculae future position
                //1. how long will it take for me to reach the enemy?
                float timeToReachEnemy = distanceToEnemy / movement.GetMaxSpeed();
                //2. where will the enemy be after this time
                Vector3 futurePosition = nearestEnemyPosition + enemyMovement.GetCurrentVelocity() * timeToReachEnemy;

                //if the future positionof the target is behind the follower, he should not run backward

                if (Vector3.Angle((futurePosition - myPosition), (nearestEnemyPosition - myPosition)) > 90)
                {
                    movement.MoveTo(nearestEnemyPosition + (myPosition - nearestEnemyPosition).normalized * (perfectMeleeDistance + myWidth + enemyWidth));
                }
                else
                {
                    movement.MoveTo(futurePosition);
                }


            }
            else
            {
                movement.MoveTo(nearestEnemyPosition + (myPosition - nearestEnemyPosition).normalized * (perfectMeleeDistance + myWidth + enemyWidth));
            }

            if ((nearestEnemyPosition - myPosition).sqrMagnitude > maxMeleeDistance)
            {
                inRange = false;
            }
            else
            {
                inRange = true;
            }
        }

        /*if (inRange)
        {

            //electrofield does the job on itself - do we need it at the bahaviour?
        }*/

    }

    public override void OnBehaviourEnter()
    {
        movement.ActivateLookAt();
    }

    public override void OnBehaviourExit()
    {
        movement.Stop();
        movement.StopLookAt();
        inRange = false;
    }
}


[System.Serializable]
public class B_Tick : Behaviour
{
    EC_Sensing enemySensing;
    EC_Movement movement;
    Explosives explosives;

    public float distanceCheckingInterval;
    float nextDistanceCheckTime;

    //explodes at this distance  
    public float explosionDistance;

    float myWidth;
    float enemyWidth;



    public void SetUpBehaviour(GameEntity entity, EC_Movement movement, EC_Sensing enemySensing, Explosives explosives)//, EC_ElectroField )
    {
        this.entity = entity;
        this.movement = movement;
        this.enemySensing = enemySensing;
        this.explosives = explosives;

        nextDistanceCheckTime = UnityEngine.Random.Range(0, distanceCheckingInterval);
        explosionDistance *= explosionDistance;
    }


    protected override void Update()
    {
        //movement.LookAt(enemySensing.nearestEnemy.GetPositionForAiming() - entity.GetPositionForAiming());

        if (Time.time > nextDistanceCheckTime)
        {
            nextDistanceCheckTime += distanceCheckingInterval;

            myWidth = entity.width;
            enemyWidth = enemySensing.nearestEnemy.width;

            Vector3 nearestEnemyPosition = enemySensing.nearestEnemy.transform.position;
            Vector3 myPosition = entity.transform.position;

            float widthFactor = myWidth + enemyWidth; //multiply the resulting distanceVectorBythisFactor to also use width
            Vector3 distanceVector = nearestEnemyPosition - myPosition;
            float distanceToEnemySquared = (distanceVector - distanceVector.normalized * widthFactor).sqrMagnitude;
           
            movement.MoveTo(nearestEnemyPosition + (myPosition - nearestEnemyPosition).normalized * (myWidth + enemyWidth));

            //if the enemy is moving, we move to the position he will be at the time we arrive
            // EC_Movement enemyMovement = enemySensing.nearestEnemy.GetComponent<EC_Movement>();


            /* no heuristic for now
            if (enemyMovement.IsMoving())
            {
                //heuristically calculae future position
                //1. how long will it take for me to reach the enemy?
                float timeToReachEnemy = distanceToEnemy / movement.GetMaxSpeed();
                //2. where will the enemy be after this time
                Vector3 futurePosition = nearestEnemyPosition + enemyMovement.GetCurrentVelocity() * timeToReachEnemy;

                //if the future positionof the target is behind the follower, he should not run backward

                if (Vector3.Angle((futurePosition - myPosition), (nearestEnemyPosition - myPosition)) > 90)
                {
                    movement.MoveTo(nearestEnemyPosition + (myPosition - nearestEnemyPosition).normalized * (perfectMeleeDistance + myWidth + enemyWidth));
                }
                else
                {
                    movement.MoveTo(futurePosition);
                }


            }
            else
            {
                movement.MoveTo(nearestEnemyPosition + (myPosition - nearestEnemyPosition).normalized * (perfectMeleeDistance + myWidth + enemyWidth));
            }*/

            if (distanceToEnemySquared < explosionDistance)
            {
                explosives.Explode();
            }
        }


    }

    public override void OnBehaviourEnter()
    {
        //movement.ActivateLookAt();
    }

    public override void OnBehaviourExit()
    {
        movement.Stop();
        //movement.StopLookAt();
    }
}

[System.Serializable]
public class B_MissileFighterInCover : Behaviour
{
    //refactor and add distance checking with width

    //EC_Movement movement;
    EC_Sensing enemySensing;
    EC_MissileWeaponController weaponController;
    Animator handsAnimator;
    EC_HumanWeaponSystem weaponSystem;
    //goes to nearest enemy, shoots  and looks at them when in range, tries not to get too close

    /*public float perfectShootingDistance;
    public float maxShootingDistance;
    bool inRange;

    float myWidth;
    float enemyWidth;


    public float distanceCheckingInterval;
    float nextDistanceCheckTime;*/

    [Tooltip("to make the bahaviour more realistical, the unit starts shooting after a small delay after entering this behaviour")]
    public float shootingStartDelay;
    float shootingStartTime;
    bool shootingStarted;

    public void SetUpBehaviour(GameEntity entity, EC_Sensing enemySensing, EC_MissileWeaponController weapon)
    {
        this.enemySensing = enemySensing;
        this.entity = entity;
        //this.movement = movement;
        this.weaponController = weapon;
    }

    public void SetUpBehaviour(GameEntity entity, EC_Sensing enemySensing, EC_MissileWeaponController weapon, Animator handsAnimator, EC_HumanWeaponSystem weaponSystem)
    {
        this.enemySensing = enemySensing;
        this.entity = entity;
        //this.movement = movement;
        this.weaponController = weapon;
        this.handsAnimator = handsAnimator;
        this.weaponSystem = weaponSystem;
    }

    protected override void Update()
    {
     
        //weaponController.StopAiming();

        weaponController.AimAt(enemySensing.nearestEnemy);


        if (!shootingStarted)
        {
            if (Time.time > shootingStartTime) shootingStarted = true;
        }
         else
         {

                 if (weaponController.HasEnoughAmmoLoaded())
                 {
                     if (weaponSystem.CanShoot())
                     {

                         weaponController.Shoot();


                         //Debug.Log("shot on state: " + weaponSystem.state);
                     }

                 }
                 else if (!weaponSystem.IsReloading())
                 {
                     //Debug.Log("not enough ammo");
                     //weaponSystem.StartReloading();
                     //Debug.Log("reload");
                 }

         }
    }

    public override void OnBehaviourExit()
    {
        /*if (handsAnimator != null)
        {
            handsAnimator.SetTrigger("EnterIdleStanceMissile1");
        }*/
        //movement.Stop();
        //movement.StopLookAt();
        weaponController.StopAiming();

       // inRange = false;

        if (handsAnimator != null)
        {
            // handsAnimator.SetBool("CombatStance2", false);

        }
    }

    public override void OnBehaviourEnter()
    {
        shootingStarted = false;
        shootingStartTime = Time.time + shootingStartDelay;

        if (handsAnimator != null)
        {
            //handsAnimator.SetBool("CombatStance2", true);
            handsAnimator.SetBool("IdleStance", false);

        }
    }
}