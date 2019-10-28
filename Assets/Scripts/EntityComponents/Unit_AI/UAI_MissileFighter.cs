﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAI_MissileFighter : EC_UnitAI
{
    public B_MissileFighter missileBehaviour;

    public EC_Movement movement;
    public EC_Sensing sensing;
    public EC_MissileWeaponController weapon;
    public Animator handsAnimator;

    // Start is called before the first frame update
    public override void SetUpComponent(GameEntity entity)
    {
        base.SetUpComponent(entity);
        currentBehaviour = null;
        missileBehaviour.SetUpBehaviour(entity, movement, sensing, weapon, handsAnimator);
    }

    public override void CheckCurrentBehaviour()
    {
        if (sensing.nearestEnemy != null)
        {
            SetCurrentBehaviour(missileBehaviour);
        }
        else
        {
            SetCurrentBehaviour(null);
        }
    }
}
