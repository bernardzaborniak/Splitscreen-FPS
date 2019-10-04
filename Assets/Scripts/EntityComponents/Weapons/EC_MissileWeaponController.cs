﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* 
 * later on , make either a type enum or children - for drawable - bow and magic and - reloadable - crossbow or gun weapons
 */ 
public class EC_MissileWeaponController : EntityComponent
{
    //public float shootingInterval;
    //float nextShootingTime;
    //public float damage;
    public MissileWeapon weapon;

    [Tooltip("how fast do we aim")]
    public float turningSpeed;
    //public GameObject projectilePrefab;
    //public Transform projectileSpawnPoint;

    public Transform parent;
    bool aiming = false;
    GameEntity currentTarget;

    public bool gravityProjectile;
    //projectuile needs to weight 1kg?
    //public float initialLaunchSpeed;
    bool hasMovement; //does currentTarget have movement?
    EC_Movement currentEnemyMovement;

    [Tooltip("the bullet gets rotated randomly upon shooting, so we add some skill based aiming")]
    public float shootingError;

    public bool reloading = false;
    float reloadEndTime;

    /*public override void SetUpComponent(GameEntity entity)
    {
        base.SetUpComponent(entity);
        nextShootingTime = Time.time + shootingInterval;
    }*/

    public override void UpdateComponent()
    {
        if (aiming)
        {
            //tod ofix shaking of wepon when enemy is to near on gravity shot someday
            //use weapon transform for rotation, use spwanpoint position for  distance checks

            if (currentTarget != null)
            {
                Vector3 desiredAimVector = Vector3.zero;


                if (!gravityProjectile)
                {
                    //if the projectile flies straight with no gravity, we just calculate the time in air and aim there  


                    if (hasMovement)
                    {
                        float timeInAir = (currentTarget.GetPositionForAiming() - weapon.GetProjectileSpawnPoint()).magnitude / weapon.initialLaunchSpeed;

                        //now calculate again, but with speed of the target added
                        timeInAir = ((currentTarget.GetPositionForAiming() + currentEnemyMovement.GetCurrentVelocity() * timeInAir) - weapon.GetProjectileSpawnPoint()).magnitude / weapon.initialLaunchSpeed;

                        desiredAimVector = (currentTarget.GetPositionForAiming()+currentEnemyMovement.GetCurrentVelocity()*timeInAir) - weapon.transform.position;
                    }
                    else
                    {
                        desiredAimVector = currentTarget.GetPositionForAiming() - weapon.transform.position;
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

                        desiredAimVector = (enemyPosition + currentEnemyMovement.GetCurrentVelocity() * timeInAir)- weaponPosition;
                        desiredAimVector.y = 0;
                        desiredAimVector = Quaternion.AngleAxis(-launchAngle, weapon.transform.right) * desiredAimVector;
                        //Debug.Log("has movement: " + launchAngle);

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
                        
                        //desiredAimVector = Quaternion.AngleAxis(-launchAngle, projectileSpawnPoint.right) * desiredAimVector;
                        desiredAimVector = enemyPosition - weaponPosition;
                        desiredAimVector.y = 0;
                        desiredAimVector = Quaternion.AngleAxis(-launchAngle, weapon.transform.right) * desiredAimVector;
                        //Debug.Log("no movement: " + launchAngle);

                    }




                }

                RotateTowards(desiredAimVector);
            }
        }
        else
        {
            RotateTowards(parent.forward);
        }

        if (reloading)
        {
            if (Time.time > reloadEndTime)
            {
                reloading = false;
                weapon.Reload();
            }
        }
    }

    public void Shoot()
    {
        //add random rotation based on skill

        weapon.HandleWeaponKey(0);
        /*Projectile projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation * Quaternion.Euler(Random.Range(-shootingError, shootingError), Random.Range(-shootingError, shootingError), 0f)).GetComponent<Projectile>();
        projectile.startVelocity = initialLaunchSpeed;
        projectile.projectileTeamID = myEntity.teamID;
        projectile.damage = damage;
        nextShootingTime = Time.time + shootingInterval;*/
    }

    public bool CanShoot()
    {
        /*if (Time.time > nextShootingTime)
        {
            return true;
        }
        else
        {
            return false;
        }*/
        if (weapon.currentMagazineAmmo > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public void Reload()
    {
        reloading = true;
        reloadEndTime = Time.time + weapon.reloadTime;
    }

    public void AimAt(GameEntity target)
    {
        aiming = true;
        currentTarget = target;
        currentEnemyMovement = target.GetComponent<EC_Movement>();
        if (currentEnemyMovement != null) hasMovement = true;
        else hasMovement = false;

    }

    /*public void AimAt(Vector3 position)
    {
        aiming = true;
    }*/

    public void StopAiming()
    {
        aiming = false;
    }


    void RotateTowards(Vector3 desiredLookVector)
    {
        //Quaternion desiredLookRotation = Quaternion.LookRotation(position - turrenRotatingBarrel.transform.position);
        Quaternion desiredLookRotation = Quaternion.LookRotation(desiredLookVector);
        weapon.transform.rotation = Quaternion.RotateTowards(weapon.transform.rotation, desiredLookRotation, turningSpeed*Time.deltaTime);

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
