﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class APActionNode
{
    public APActionNode _parent;
    public APGameState _gameState;
    public int _score = 0;
    protected ActionPointPanel _actionPointPanel;

    public abstract void Perform();
    public abstract int GetScoreToAdd(APGameState prevState);
    public virtual bool ShouldReplan(List<Unit> units, List<Cube> cubes)
    {
        //*** _gameState와 currGameState가 같은지 확인 ***//

        // 유닛의 수가 같은지 확인
        if (units.Count != _gameState._units.Count)
            return true;

        // 액션포인트가 다른지 확인
        if (_gameState.self.actionPoint != units.Find(u => u == _gameState.self.owner).actionPointsRemain)
            return true;

        // 유닛들의 체력이 같은지 확인
        if (!UnitHealthValidation(units))
            return true;

        // 같은 큐브에 같은 유닛이 배치되어있는지 확인
        if (!UnitPosValidation(units, cubes))
            return true;

        return false;
    }

    protected bool UnitPosValidation(List<Unit> units, List<Cube> cubes)
    {
        // 같은 큐브에 같은 유닛이 배치되어있는지 확인
        foreach (Unit unit in units)
        {
            // plan속 유닛
            APUnit apUnit = _gameState._units.Find(u => u.owner == unit);

            // plan에는 유닛이 있지만 실제론 없음
            if (apUnit == null)
                return false;

            // 실제론 unit이 존재하는 plan에는 없음
            if (_gameState._unitPos.TryGetValue(apUnit, out _) == false)
                return false;

            // 실제와 plan 둘다 해당 유닛이 있는데 
            Cube cubeInPlan;
            if (_gameState._unitPos.TryGetValue(apUnit, out cubeInPlan))
            {
                // 다른 큐브에 있다면
                if (cubeInPlan != unit.GetCube)
                    return false;
            }
        }

        return true;
    }

    protected bool UnitHealthValidation(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            APUnit simulUnit = _gameState._units.Find(u => u.owner == unit);
            if (simulUnit == null) // 실제 유닛은 있지만 plan속 유닛이 없는 경우
                return false;

            if (simulUnit.health != unit.Health) // 둘다 있지만 체력이 다른 경우
                return false;
        }
        return true;
    }

    public abstract void OnWaitEnter();

    public abstract void OnWaitExecute();

    public abstract void OnWaitExit();
}



public class RootNode : APActionNode
{
    public RootNode(APGameState gameState)
    {
        _parent = null;
        _gameState = gameState;
        _score = 0;
    }

    public override void OnWaitEnter()
    {
    }

    public override void OnWaitExecute()
    {
    }

    public override void OnWaitExit()
    {
    }

    public override void Perform() { }

    public override int GetScoreToAdd(APGameState prevState) => 0;

    public override bool ShouldReplan(List<Unit> units, List<Cube> cubes) => false;
}





public class ActionNode_Move : APActionNode
{
    PFPath _path;
    Cube _destination;
    public ActionNode_Move(APGameState prevGameState, int prevScore, ActionPointPanel actionPointPanel, PFPath path)
    {
        _gameState = prevGameState.Clone();
        _score = prevScore;

        _path = new PFPath(path.start, path.destination);
        _path.path.AddRange(path.path);
        _destination = _gameState._cubes.Find(c => c == path.destination as Cube);
        _actionPointPanel = actionPointPanel;

        // 가상 GameState도 바꿔주기
        _gameState.self.actionPoint -= _gameState.self.owner.CalcMoveAPCost(_path);
        _gameState.MoveTo(_destination);

        // 바꾼 GameState와 prev를 비교하여 점수계산
        _score += GetScoreToAdd(prevGameState);
    }

    public override void Perform()
    {
        _gameState.self.owner.MoveTo(_path);
    }

    public override void OnWaitEnter()
    {
        (_path.destination as Cube).SetBlink(0.5f);
    }

    public override void OnWaitExecute()
    {
        _actionPointPanel.SetText(_gameState.self.owner.actionPointsRemain);
    }

    public override void OnWaitExit()
    {
        (_path.destination as Cube).StopBlink();
    }

    public override int GetScoreToAdd(APGameState prevState)
    {
        // 적과 가까워지면 +100

        // 이전 거리들중 가장 가까운 적을 선택
        Team selfTeam = prevState.self.owner.team;
        List<APUnit> prevEUnits = new List<APUnit>(prevState._units.Where(u => u != prevState.self && u.owner.team.enemyTeams.Contains(selfTeam)));
        Vector3 prevSelfPos = prevState._unitPos[prevState.self].Platform.position;
        prevSelfPos.y = 0f;
        APUnit prevClosestEUnit = prevEUnits.Aggregate((acc, cur) => {
            Cube eCube_cur = prevState._unitPos[cur];
            Vector3 ePos_cur = eCube_cur.Platform.position;
            ePos_cur.y = 0f;
            float curDist = Vector3.Distance(ePos_cur, prevSelfPos);

            Cube eCube_acc = prevState._unitPos[acc];
            Vector3 ePos_acc = eCube_acc.Platform.position;
            ePos_acc.y = 0f;
            float accDist = Vector3.Distance(ePos_acc, prevSelfPos);

            return curDist < accDist ? cur : acc;
        });
        Vector3 prevCEUPos = prevState._unitPos[prevClosestEUnit].Platform.position;
        prevCEUPos.y = 0f;
        float prevDist = Vector3.Distance(prevSelfPos, prevCEUPos);

        // 현재에서 얼마나 가까워졌는지
        APUnit currClosestEUnit = _gameState._units.Find(u => u.owner == prevClosestEUnit.owner);
        Vector3 currCEUPos = _gameState._unitPos[currClosestEUnit].Platform.position;
        currCEUPos.y = 0f;
        Vector3 currSelfPos = _gameState._unitPos[_gameState.self].Platform.position;
        currSelfPos.y = 0f;
        float currDist = Vector3.Distance(currSelfPos, currCEUPos);


        return (int)(100 * Mathf.Max(0, prevDist - currDist));
    }
}



public class ActionNode_Attack : APActionNode
{
    Unit _target;
    MapMgr _mapMgr;
    bool couldntAttack = false;
    public ActionNode_Attack(APGameState prevGameState, int prevScore, ActionPointPanel actionPointPanel, Unit target, MapMgr mapMgr)
    {
        _gameState = prevGameState.Clone();
        _score = prevScore;
        _actionPointPanel = actionPointPanel;

        _target = target;
        _mapMgr = mapMgr;

        // 가상 gameState 변경
        _gameState.self.actionPoint -= _gameState.self.owner.GetActionSlot(ActionType.Attack).cost;
        _gameState.Attack(_gameState.APFind(target));

        // 바꾼 GameState와 prev를 비교하여 점수계산
        _score += GetScoreToAdd(prevGameState);
    }

    public override void Perform()
    {
        Unit unit = _gameState.self.owner;

        Cube targetCube = _target.GetCube;
        List<Cube> cubesInAttackRange = _mapMgr.GetCubes(
                unit.basicAttackRange.range,
                unit.basicAttackRange.centerX,
                unit.basicAttackRange.centerZ,
                targetCube
            );

        // 공격하고자 했던 유닛이 범위내에 없다면 공격 실패
        // 추후 Replan을 위해 couldntAttack = true;
        if (targetCube == null || targetCube.GetUnit() == null || !cubesInAttackRange.Contains(targetCube))
        {
            couldntAttack = true;
            return;
        }

        // 실제 공격
        List<Cube> cubesToAttack = _mapMgr.GetCubes(
            unit.basicAttackSplash.range,
            unit.basicAttackSplash.centerX,
            unit.basicAttackSplash.centerX,
            _target.GetCube);

        unit.Attack(
                cubesToAttack,
                _target.GetCube
            );
    }

    public override void OnWaitEnter()
    {
        _target.GetCube.SetBlink(0.7f);
    }

    public override void OnWaitExecute()
    {
        _actionPointPanel.SetText(_gameState.self.owner.actionPointsRemain);
    }

    public override void OnWaitExit()
    {
        _target.GetCube.StopBlink();
    }

    public override bool ShouldReplan(List<Unit> units, List<Cube> cubes)
    {
        if(couldntAttack)
            return true;
        else
            return base.ShouldReplan(units, cubes);
    }

    public override int GetScoreToAdd(APGameState prevState)
    {
        // 체력이 많은 적보다
        // 체력이 조금 남은 적을 공격하기 (+ 2000 * (자신의공격력/적의체력))
        int score = (int)(2000 * (
            _gameState.self.owner.BasicAttackDamageAvg /
            (float)prevState._units.Find(u => u.owner == _target).health
            ));

        // 적을 죽이는 plan이면 +2000
        if (prevState._units.Count - 1 == _gameState._units.Count)
            score += 2000;

        return score;
    }
}

