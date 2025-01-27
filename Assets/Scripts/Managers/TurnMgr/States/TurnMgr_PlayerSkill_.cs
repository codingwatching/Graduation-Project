using ObserverPattern;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnMgr_PlayerSkill_ : TurnMgr_State_
{
    List<Cube> cubesCanCast;
    List<Cube> cubesCastRange;
    Cube cubeClicked;

    public TurnMgr_PlayerSkill_(TurnMgr owner, Unit unit) : base(owner, unit)
    {
        // get all cubes in range
        cubesCastRange = MapMgr.Instance.GetCubes(
            unit.skill.skillRange,
            unit.GetCube
            );

        // filter cubes
        cubesCanCast = cubesCastRange
            .Where(CubeCanCastConditions)
            .ToList();
    }
    public override void Enter()
    {
        CameraMgr.Instance.SetTarget(unit, true);

        MapMgr.Instance.BlinkCubes(cubesCastRange, 0.3f);
        MapMgr.Instance.BlinkCubes(cubesCanCast, 0.7f);
        unit.StartBlink();

        UIMgr.Instance.SetUIComponent<TMBackBtn>();

        EventMgr.Instance.onTurnActionEnter.Invoke();
    }

    public override void Execute()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (RaycastWithCubeMask(out hit))
            {
                Cube cubeClicked = hit.transform.GetComponent<Cube>();
                if (cubesCanCast.Contains(cubeClicked))
                {
                    List<Cube> cubesInSkillSplash = MapMgr.Instance.GetCubes(
                        unit.skill.skillSplash, cubeClicked);

                    // 스킬은 유닛이 없는 곳에 구사가능
                    string popupContent = "r u sure you wanna use Skill here?";

                    owner.stateMachine.ChangeState(
                        new TurnMgr_Popup_(owner, unit, Input.mousePosition, popupContent, 
                        ()=>CastSkillOnCube(cubeClicked), OnClickNo, () => cubesInSkillSplash.ForEach(c => c.SetBlink(0.7f)), null, () => MapMgr.Instance.StopBlinkAll()),
                        StateMachine<TurnMgr>.StateTransitionMethod.JustPush);
   
                }
                else
                {
                    AudioMgr.Instance.PlayAudio(AudioMgr.AudioClipType.UI_NoAccept, AudioMgr.AudioType.UI);
                }

            }
            else
            {
                AudioMgr.Instance.PlayAudio(AudioMgr.AudioClipType.UI_NoAccept, AudioMgr.AudioType.UI);
            }
        }
    }

    public override void Exit()
    {
        unit.StopBlink();
        MapMgr.Instance.StopBlinkAll();
        EventMgr.Instance.onTurnActionExit.Invoke();
    }
    private bool CubeCanCastConditions(Cube cube)
        => true; // 범위내의 모든 큐브에 Cast가능합니다.

    private void CastSkillOnCube(Cube cubeClicked)
    {
        this.cubeClicked = cubeClicked;

        TurnMgr_State_ nextState = new TurnMgr_PlayerBegin_(owner, unit);
        owner.stateMachine.ChangeState(
            new TurnMgr_WaitSingleEvent_(
                owner, unit, EventMgr.Instance.onUnitSkillExit, nextState, 
                (param) => ((UnitStateEvent)param)._owner == unit),
            StateMachine<TurnMgr>.StateTransitionMethod.JustPush);

        unit.StopBlink();

        SkillCommand skillCommand;
        if(SkillCommand.CreateCommand(unit, cubeClicked, out skillCommand))
        {
            unit.EnqueueCommand(skillCommand);
        }
    }

    private void OnClickNo() => owner.stateMachine.ChangeState(null, StateMachine<TurnMgr>.StateTransitionMethod.ReturnToPrev);

    private bool RaycastWithCubeMask(out RaycastHit hit)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Cube"));
    }
}
