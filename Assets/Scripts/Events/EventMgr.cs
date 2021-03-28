﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventMgr : SingletonBehaviour<EventMgr>
{

    [Header("PathFinding Event")]
    [SerializeField] public Event onPathfindRequesterCountZero;
    [SerializeField] public Event onPathUpdatingStart;

    [Header("Unit Event")]
    [SerializeField] public Event onUnitSummonReady;
    [SerializeField] public Event onUnitSummonEnd;

    [SerializeField] public Event onUnitAttackExit;
    [SerializeField] public Event onUnitDeadEnter;
    [SerializeField] public Event onUnitDeadExit;
    [SerializeField] public Event onUnitIdleEnter;
    [SerializeField] public Event onUnitItemExit;
    [SerializeField] public Event onUnitRunEnter;
    [SerializeField] public Event onUnitRunExit;
    [SerializeField] public Event onUnitSkillExit;
    [SerializeField] public Event onUnitDeadCountZero;

    [Header("Turn Event")]
    [SerializeField] public Event onTurnActionEnter; // begin, popup을 제외한 모든 TurnState
    [SerializeField] public Event onTurnActionExit;
    [SerializeField] public Event onTurnBeginEnter;
    [SerializeField] public Event onTurnBeginExit;
    [SerializeField] public Event onTurnItemEnter; 
    [SerializeField] public Event onTurnItemExit;
    [SerializeField] public Event onTurnMove;
    [SerializeField] public Event onTurnNobody;
    [SerializeField] public Event onTurnPlan; 

    [Header("Game Event")]
    [SerializeField] public Event onGameBattleEnter;
    [SerializeField] public Event onGameBattleExit;
    [SerializeField] public Event onGameInitEnter;
    [SerializeField] public Event onGameInitExit;
    [SerializeField] public Event onGamePositioningEnter;
    [SerializeField] public Event onGamePositioningExit;
    
    
}
