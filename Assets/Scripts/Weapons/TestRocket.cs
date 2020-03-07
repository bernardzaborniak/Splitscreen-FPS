﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRocket : MonoBehaviour
{
    public float explosionRadius;
    public float pushForce;
    public float killPushForce;
    public int projectileTeamID;
    public float damage;
    [Tooltip("adds force upwards - the units fly upwards - unrealistic, but looks better")]
    public float upwardsLifter;
    public GameObject explosionVisualsPrefab;
    public float explosionVisualsDestroyDelay;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Boom();
        }
    }

    protected  void Boom()
    {
        //make a sphereCast in the explosionRADIUS
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, explosionRadius);
        float squaredMaxDistance = explosionRadius * explosionRadius;


        for (int i = 0; i < collidersInRange.Length; i++)
        {
            IDamageable<DamageInfo> damageable = collidersInRange[i].gameObject.GetComponent<IDamageable<DamageInfo>>();
            IPusheable<Vector3> pusheable = collidersInRange[i].gameObject.GetComponent<IPusheable<Vector3>>();

            Vector3 pushDirection = collidersInRange[i].transform.position - transform.position;
            //between 1 when in center and 0 when on the ends - linear
            float distanceSquared = pushDirection.sqrMagnitude;
            float distanceModifier = Utility.Remap(distanceSquared, 0, squaredMaxDistance, 1, 0);

            if (damageable != null)
            {
                // Debug.Log("hit");
                // check who did we hit, check if he has an gameEntity
                GameEntity entity = collidersInRange[i].gameObject.GetComponent<GameEntity>();
                if (entity != null)
                {
                    if (!SceneSettings.Instance.friendlyFire)
                    {
                        DiplomacyStatus diplomacyStatus = SceneSettings.Instance.GetDiplomacyStatus(projectileTeamID, entity.teamID);
                        if (diplomacyStatus == DiplomacyStatus.War)
                        {
                            GiveDamage(damageable, pushDirection, distanceModifier);
                        }

                    }
                    else
                    {
                        GiveDamage(damageable, pushDirection, distanceModifier);
                    }

                }
                else
                {
                    GiveDamage(damageable, pushDirection, distanceModifier);
                }
            }

            if (pusheable != null)
            {
                //Debug.Log(collidersInRange[i].gameObject + "dist: " + distanceSquared + " mod: " + distanceModifier + "force: " + pushForce * distanceModifier);
                pusheable.Push((pushDirection.normalized * pushForce  + Vector3.up*upwardsLifter)*distanceModifier * SceneSettings.Instance.forceMultiplier);

            }

        }


        GameObject visuals = Instantiate(explosionVisualsPrefab, transform.position, transform.rotation);
        Destroy(visuals, explosionVisualsDestroyDelay);


        //gameObject.SetActive(false);

    }

    protected void GiveDamage(IDamageable<DamageInfo> damageable, Vector3 pushDirection, float distanceModifier)
    {
        damageable.TakeDamage(new DamageInfo(damage * distanceModifier, (pushDirection.normalized * killPushForce + Vector3.up * upwardsLifter) * distanceModifier, null));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
