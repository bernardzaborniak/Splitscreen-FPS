﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EC_PlayerWeaponSystem : EC_HumanWeaponSystem
{
    [Header("Visualisation")]
    public AimVisualiser aimVisualiser;

    public WeaponHUD weaponHUD;

    [Header("Starting Weapons")]
    public GameObject startingWeapon1;
    public GameObject startingWeapon2;
    public GameObject startingWeapon3;



    public override void SetUpComponent(GameEntity entity)
    {
        ResetWeapons();
        base.SetUpComponent(entity);
       
        if (weaponHUD != null) weaponHUD.SetUp(this);
    }

    public override void UpdateComponent()
    {
        base.UpdateComponent();

        if (currentSelectedWeapon != null)
        {
            if (currentSelectedWeapon.usesAimingLine)
            {
                MissileWeapon mw = currentSelectedWeapon as MissileWeapon;
                aimVisualiser.DrawLine(mw.GetProjectileSpawnPoint(), mw.transform.forward, 15, mw.bloom);
            }
        }


        if (weaponHUD != null)
        {
            weaponHUD.UpdateHUD(currentSelectedWeapon);
        }
    }

    #region UseWeapon
    public void UseWeaponStart(int actionID)
    {
        if(state == WeaponSystemState.Default)
        {
            if (currentSelectedWeapon != null)
            {
                if(currentSelectedWeapon is MeleeWeapon)
                {
                    meleeWeaponControler.MeleeAttack();
                }
                else
                {
                    currentSelectedWeapon.HandleWeaponKeyDown(actionID);
                }
            }
        }
       
    }

    public void UseWeaponHold(int actionID)
    {
        if (state == WeaponSystemState.Default)
        {
            if (currentSelectedWeapon != null)
            {
                 currentSelectedWeapon.HandleWeaponKeyHold(actionID);
            }
        }
    }

    public void UseWeaponEnd(int actionID)
    {
        if (state == WeaponSystemState.Default)
        {
            if (currentSelectedWeapon != null)
            {
                 currentSelectedWeapon.HandleWeaponKeyUp(actionID);
            }
        }
    }

    #endregion

    #region SelectingWeapons

    public void SelectNextWeapon()
    {
        if (currentSelectedWeaponID == inventory.Length - 1)
        {
            ChangeWeapon(0);
        }
        else
        {
            ChangeWeapon(currentSelectedWeaponID + 1);
        }
    }

    public void SelectPreviousWeapon()
    {
        if (currentSelectedWeaponID == 0)
        {
            ChangeWeapon(inventory.Length - 1);
        }
        else
        {
            ChangeWeapon(currentSelectedWeaponID - 1);
        }
    }

    #endregion

    public void ResetWeapons()
    {
        //first delete all weapons currently there
        foreach(Transform transform in rightHand)
        {
            Destroy(transform.gameObject);
        }


        GameObject weapon1;
        GameObject weapon2;
        GameObject weapon3;


        if (startingWeapon1 != null)
        {
            weapon1 = Instantiate(startingWeapon1, rightHand);
            inventory[0] = weapon1.GetComponent<Weapon>();
        }
        if (startingWeapon2 != null)
        {
            weapon2 = Instantiate(startingWeapon2, rightHand);
            inventory[1] = weapon2.GetComponent<Weapon>();
        }
        if (startingWeapon3 != null)
        {
            weapon3 = Instantiate(startingWeapon3, rightHand);
            inventory[2] = weapon3.GetComponent<Weapon>();
        }

 
    }

    
}
