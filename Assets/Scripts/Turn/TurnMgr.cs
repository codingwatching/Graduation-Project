﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnMgr : MonoBehaviour
{
    [SerializeField] public MapMgr mapMgr;
    [SerializeField] private List<Unit> units = new List<Unit>();
    [SerializeField] public Queue<Unit> turns = new Queue<Unit>();
    [SerializeField] public GameObject actionPanel;
    [SerializeField] public Event e_onUnitRunExit;
    [SerializeField] public Event e_onClickMoveBtn;

    public StateMachine<TurnMgr> stateMachine;
    public enum TMState { Nobody, PlayerTurnBegin, PlayerTurnMove, PlayerTurnAttack, AI }
    public TMState tmState;

    private void Reset()
    {
        units.Clear();
        units.AddRange(FindObjectsOfType<Unit>());
        mapMgr = FindObjectOfType<MapMgr>();
    }

    public void Start()
    {
        mapMgr = FindObjectOfType<MapMgr>();
        units.Clear();
        units.AddRange(FindObjectsOfType<Unit>());
        turns.Clear();
        foreach(var u in units.OrderByDescending((u) => u.agility))
        {
            Debug.Log(u.gameObject.name);
            turns.Enqueue(u);
        }

        stateMachine = new StateMachine<TurnMgr>(new NobodyTurn(this));
    }

    private void Update()
    {
        stateMachine.Run();
        // 디버깅용
        CheckTurnState();
    }

    /// <summary>
    /// 외부에서는 stateMachine을 직접 접근하지말고 이 메서드를 사용하세요.
    /// </summary>
    /// <param name="state">다음 State</param>
    public void ChangeState(State<TurnMgr> state)
    {
        stateMachine.ChangeState(state, StateMachine<TurnMgr>.StateChangeMethod.PopNPush);
    }

    public void NextTurn()
    {
        if (!stateMachine.IsStateType(typeof(NobodyTurn)))
        {
            Unit unitPrevTurn = turns.Dequeue();
            turns.Enqueue(unitPrevTurn);
        }


        Unit unitToHaveTurn = turns.Peek();

        if(unitToHaveTurn.team == TeamMgr.Team.Player)
            stateMachine.ChangeState(new PlayerTurnBegin(this, unitToHaveTurn), StateMachine<TurnMgr>.StateChangeMethod.PopNPush);

        else if(unitToHaveTurn.team == TeamMgr.Team.AI)
            stateMachine.ChangeState(new AITurn(this, unitToHaveTurn), StateMachine<TurnMgr>.StateChangeMethod.PopNPush);
    }

    // 디버깅용
    private void CheckTurnState()
    {
        if (stateMachine.IsStateType(typeof(PlayerTurnBegin)))
            tmState = TMState.PlayerTurnBegin;

        else if(stateMachine.IsStateType(typeof(PlayerTurnMove)))
            tmState = TMState.PlayerTurnMove;

        else if (stateMachine.IsStateType(typeof(PlayerTurnAttack)))
            tmState = TMState.PlayerTurnAttack;

        else if (stateMachine.IsStateType(typeof(AITurn)))
            tmState = TMState.AI;

        else
            tmState = TMState.Nobody;
    }
}