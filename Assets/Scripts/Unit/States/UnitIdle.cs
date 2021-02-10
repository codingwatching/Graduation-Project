﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitIdle : State<Unit>
{
    public UnitIdle(Unit owner) : base(owner) { }

    public override void Enter()
    {
        owner.anim.SetTrigger("ToIdle");
    }

    public override void Execute()
    {
        if (owner.currHealth <= 0) 
            owner.stateMachine.ChangeState(new UnitDead(owner), StateMachine<Unit>.StateChangeMethod.PopNPush);
    }

    public override void Exit()
    {
        
    }
}
