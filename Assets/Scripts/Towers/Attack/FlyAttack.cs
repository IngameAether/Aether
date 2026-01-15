using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyAttack : Fly
{
    [Space]
    [Header("Attack Variables")]
    [SerializeField] private AnimationClip attackAnim;
    [SerializeField] private float AttackAnimSpeed;
    [SerializeField] private bool needRotation;
    private bool isFlyingEnd = false;

    public override void Init(Vector2 tower, Transform target, float damage)
    {
        base.Init(tower, target, damage);
    }

    public override float GetAroundTime()
    {
        aroundTime = attackAnim.length + flyingAnim.length;
        return aroundTime;
    }

    protected override void Update()
    {
        if (isFlyingEnd) { return; }
        else Move();
    }

    // 날아가는 거 끝나고 곧바로 Attack 애니메이션 재생
    protected override void AfterMove()
    {
        isFlyingEnd = true;
        transform.position = targetPos;
        if (!needRotation) transform.rotation = Quaternion.identity;
        PlayAttackAnim(AttackAnimSpeed);

        base.AfterMove();
        //StartCoroutine(WaitAttackEnd());
    }

    private IEnumerator WaitAttackEnd()
    {
        yield return new WaitForSeconds(attackAnim.length);
        OnTargetHit();
    }

    // Attack 애니메이션 끝날 때까지 대기
    protected override void AfterTargetHit(float t)
    {
        base.AfterTargetHit(attackAnim.length);
    }

    protected override void SetAnimationClip()
    {
        base.SetAnimationClip();

        animatorOverride["L1F_flying"] = flyingAnim;
        animatorOverride["L2F_attack"] = attackAnim;
    }
}
