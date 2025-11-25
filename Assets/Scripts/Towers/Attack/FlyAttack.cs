using System.Collections.Generic;
using UnityEngine;

public class FlyAttack : Fly
{
    [SerializeField] private AnimationClip attackAnim;
    [SerializeField] private float AttackAnimSpeed;
    private bool isFlyingEnd = false;

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
        PlayAttackAnim(AttackAnimSpeed);
    }

    // 애니메이터에서 이벤트로 호출
    private void AfterAttack()
    {
        Destroy(gameObject);
    }

    protected override void SetAnimationClip()
    {
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        animatorOverride.GetOverrides(overrides);
        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].Key.name == "Flying")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, flyingAnim);
            else if (overrides[i].Key.name == "Attack")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, attackAnim);
        }
    }
}
