﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityComponent : MonoBehaviour
{

    protected GameEntity myEntity;

    public virtual void SetUpComponent(GameEntity entity)
    {
        myEntity = entity;
    }

    public virtual void UpdateComponent()
    {

    }

    public virtual void FixedUpdateComponent()
    {

    }

    public virtual void OnDie(GameEntity killer)
    {

    }

    /*public virtual void OnTakeDamage(float damage)
    {

    }

    public virtual void OnTakeDamage(float damage, Vector3  force)
    {

    }

    public virtual void OnTakeDamageFrom(float damage, GameEntity damageGiver)
    {

    }*/

    public virtual void OnTakeDamage(DamageInfo damageInfo)
    {

    }
}
/*
//can be toogled on of via UI
public class PassiveToogleableAbility : EntityComponent
{
    public bool active;

    public void ToogleActive()
    {
        active = !active;
    }
}

//can be clicked to activate via UI
public class ActiveAbility : EntityComponent
{
    public virtual void ActivateAbility()
    {

    }
}

public class PassiveAbility : EntityComponent
{

}*/
