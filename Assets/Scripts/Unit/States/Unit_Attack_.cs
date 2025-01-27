﻿using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using ObserverPattern;

public class Unit_Attack_ : State<Unit>
{
    List<Cube> attackTargets;
    Cube centerCube;
    public Unit_Attack_(Unit owner, List<Cube> attackTargets, Cube centerCube) : base(owner) 
    {
        this.attackTargets = attackTargets;
        this.centerCube = centerCube;
    }

    public override void Enter()
    {
        owner.anim.SetTrigger("ToAttack");

        owner.LookAt(centerCube.Platform);

        int cost = owner.GetActionSlot(ActionType.Attack).cost;
        owner.actionPointsRemain -= cost;

        if (owner.projectile != null)
        {
            owner.StartCoroutine(ProcessProjectile());
        }

    }

    public override void Execute()
    {
        if (!owner.anim.GetBool("IsAttack"))
            owner.stateMachine.ChangeState(new Unit_Idle_(owner), StateMachine<Unit>.StateTransitionMethod.PopNPush);
    }

    public override void Exit()
    {
        EventMgr.Instance.onUnitAttackExit.Invoke(new UnitStateEvent(owner));
    }

    private IEnumerator ProcessProjectile()
    {
        Vector3 startPos = owner.transform.position;
        Vector3 endPos = centerCube.Platform.position;

        Quaternion startRot = Quaternion.LookRotation(endPos - startPos, Vector3.up);
        float startRotX = -30f;
        float endRotX = 30f;

        GameObject projectile = Object.Instantiate(owner.projectile, startPos, startRot);

        float currLerpTime = 0f;
        float fullLerpTime = 0.5f;
        float ShootingHeight = 1f;

        float lerp = currLerpTime / fullLerpTime;
        while (lerp <= 1f)
        {
            currLerpTime += Time.deltaTime;
            if (currLerpTime > fullLerpTime)
            {
                projectile.transform.position = endPos;
                break;
            }

            lerp = currLerpTime / fullLerpTime;

            /*  Position   */
            // Linear Lerp
            Vector3 LinearLerp = Vector3.Lerp(startPos, endPos, lerp);
            // Sin Lerp
            float ShootingLerpY = Mathf.Sin(lerp * Mathf.PI) * ShootingHeight;
            Vector3 ShootingLerp = new Vector3(0f, ShootingLerpY, 0f);

            projectile.transform.position = LinearLerp + ShootingLerp;


            /*  Rotation   */
            float rotX = Mathf.Lerp(startRotX, endRotX, lerp);
            float rotY = projectile.transform.rotation.eulerAngles.y;
            float rotZ = projectile.transform.rotation.eulerAngles.z;
            projectile.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);

            yield return null;
        }

        MonoBehaviour.Destroy(projectile);
    }


}
