﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* 
 * later on , make either a type enum or children - for drawable - bow and magic and - reloadable - crossbow or gun weapons
 */ 
public class EC_MissileWeaponController : EntityComponent
{

    //units should rotate like player only to the sides und up and down - for melee and missile
    public EC_Movement movement;
    public MissileWeapon weapon;

    //[Tooltip("how fast do we aim")]
    //public float turningSpeed;

    //public Transform parent;
    bool aiming = false;
    GameEntity currentTarget;

    public bool gravityProjectile;

    bool hasMovement; //does currentTarget have movement?
    EC_Movement currentEnemyMovement;

    [Tooltip("the bullet gets rotated randomly upon shooting, so we add some skill based aiming")]
    public float shootingError;

   /* public bool reloading = false;
    float reloadEndTime;*/

    public Animator handsAnimator; // for animating reload
    //how to solve weapon positioning without animator?
    [Tooltip("will be used if animator is disabled - thus we can not guarantee correct positioning")]
    public Transform alternativeShootPoint;

    //the weapon will not get rotated? - only the spawnpoint a bit?

        //sets new weapon
    public void SetWeapon(MissileWeapon newWeapon)
    {
        //TODO
    }

    public override void UpdateComponent()
    {
        if (aiming)
        {
            //tod ofix shaking of wepon when enemy is to near on gravity shot someday
            //use weapon transform for rotation, use spwanpoint position for  distance checks

            if (currentTarget != null)
            {
                Vector3 desiredWeaponAimVector = Vector3.zero;


                if (!gravityProjectile)
                {
                    //if the projectile flies straight with no gravity, we just calculate the time in air and aim there  


                    if (hasMovement)
                    {
                        float timeInAir = (currentTarget.GetPositionForAiming() - weapon.GetProjectileSpawnPoint()).magnitude / weapon.initialLaunchSpeed;

                        //now calculate again, but with speed of the target added
                        timeInAir = ((currentTarget.GetPositionForAiming() + currentEnemyMovement.GetCurrentVelocity() * timeInAir) - weapon.GetProjectileSpawnPoint()).magnitude / weapon.initialLaunchSpeed;

                        desiredWeaponAimVector = (currentTarget.GetPositionForAiming()+currentEnemyMovement.GetCurrentVelocity()*timeInAir) - weapon.transform.position;
                    }
                    else
                    {
                        desiredWeaponAimVector = currentTarget.GetPositionForAiming() - weapon.transform.position;
                    }
                }
                else
                {

                    Vector3 enemyPosition = currentTarget.GetPositionForAiming();
                    Vector3 launchPointPosition = weapon.GetProjectileSpawnPoint();
                    Vector3 weaponPosition = weapon.transform.position;//transform.position;

                    //refactor positions - get the m only once
                    
                    if (hasMovement)
                    {
                        //1. calculate the launch angle

                        Vector3 distDelta = enemyPosition - launchPointPosition;
                        float launchAngle = GetLaunchAngle
                        (
                            weapon.initialLaunchSpeed,
                            new Vector3(distDelta.x, 0f, distDelta.z).magnitude,
                            distDelta.y,
                            true
                        );


                        //2. calculate time in air, and adjust the rotation

                        float timeInAir;
                        float g = Physics.gravity.magnitude;
                        float vY = weapon.initialLaunchSpeed * Mathf.Sin(launchAngle * (Mathf.PI / 180));
                        //vY = 5f;
                        float startH = launchPointPosition.y;
                        float finalH = enemyPosition.y;

                        if (finalH < startH)
                        {
                            timeInAir = (vY + Mathf.Sqrt((float)(Mathf.Pow(vY, 2) - 4 * (0.5 * g) * (-(startH - finalH))))) / g;
                        }
                        else
                        {
                            float vX = weapon.initialLaunchSpeed * Mathf.Cos(launchAngle * (Mathf.PI / 180));
                            float distanceX = Vector3.Distance(enemyPosition, launchPointPosition);
                            timeInAir = distanceX / vX;
                        }

                        desiredWeaponAimVector = (enemyPosition + currentEnemyMovement.GetCurrentVelocity() * timeInAir)- weaponPosition;

                    }
                    else
                    {
                        Vector3 distDelta = enemyPosition - launchPointPosition;
                        //1. calculate the launch angle
                        float launchAngle = GetLaunchAngle
                        (
                            weapon.initialLaunchSpeed,
                            new Vector3(distDelta.x, 0f, distDelta.z).magnitude,
                            distDelta.y,
                            true
                        );
                        
                        desiredWeaponAimVector = enemyPosition - weaponPosition;

                    }




                }
                movement.LookAt(desiredWeaponAimVector);
      
            }
        }

       /* if (reloading)
        {
            if (Time.time > reloadEndTime)
            {
                reloading = false;
                weapon.EndReloading(weapon.magazineSize);
            }
        }*/
    }

    public void Shoot()
    {
        //add random rotation based on skill
        //- solve this by adding a different disturmance to the perfect aiming vector every x seconds
        Quaternion rotationBefore = weapon.transform.rotation;
        weapon.transform.rotation = weapon.transform.rotation * Quaternion.Euler(Random.Range(-shootingError, shootingError), Random.Range(-shootingError, shootingError), 0f);
        weapon.HandleWeaponKeyDown(0);
        weapon.transform.rotation = rotationBefore;
    }

    public bool HasEnoughAmmoLoaded()
    {
        if (weapon.currentMagazineAmmo > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /*public void Reload()
    {
        reloading = true;
        reloadEndTime = Time.time + weapon.reloadTime;
    }*/

    public void AimAt(GameEntity target)
    {
        aiming = true;
        movement.ActivateLookAt();
        currentTarget = target;
        currentEnemyMovement = target.GetComponent<EC_Movement>();
        if (currentEnemyMovement != null) hasMovement = true;
        else hasMovement = false;

    }


    public void StopAiming()
    {
        aiming = false;
        movement.StopLookAt();

    }



    //Formel von  https://gamedev.stackexchange.com/questions/53552/how-can-i-find-a-projectiles-launch-angle
    float GetLaunchAngle(float speed, float distance, float heightDifference, bool directShoot)
    {
        //directShoot i true dann nehmen wir die niedrigere Schussbahn, wenn false, dann eine kurvigere die mehr nach oben geht
        float theta = 0f;
        float gravityConstant = Physics.gravity.magnitude;

        if (directShoot)
        {
            theta = Mathf.Atan((Mathf.Pow(speed, 2) - Mathf.Sqrt(Mathf.Pow(speed, 4) - gravityConstant * (gravityConstant * Mathf.Pow(distance, 2) + 2 * heightDifference * Mathf.Pow(speed, 2)))) / (gravityConstant * distance));
        }
        else
        {
            theta = Mathf.Atan((Mathf.Pow(speed, 2) + Mathf.Sqrt(Mathf.Pow(speed, 4) - gravityConstant * (gravityConstant * Mathf.Pow(distance, 2) + 2 * heightDifference * Mathf.Pow(speed, 2)))) / (gravityConstant * distance));
        }

        return (theta * (180 / Mathf.PI));  //change into degrees
    }
}
