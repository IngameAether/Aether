using System.Collections.Generic;
using UnityEngine;

public class Fly : FireObjectBase
{
    [SerializeField] protected AnimationClip flyingAnim;

    [Space]
    [Header("Variables")]
    [SerializeField] private float speed;
    [SerializeField] private float flyingAnimSpeed;

    public override void Init(Vector2 tower, Transform target)
    {
        base.Init(tower, target);

        // 지정된 애니메이션으로 바꾸기 
        SetAnimationClip();

        // 방향 정하기
        Rotate();

        // flying 애니메이션 재생
        PlayFlyingAnim(flyingAnimSpeed);
    }

    protected virtual void Update()
    {
        Move();
    }

    protected void Move()
    {
        Vector2 current = transform.position;
        Vector2 dir = (targetPos - current).normalized;

        transform.position = current + dir * speed * Time.deltaTime;

        // 목표 도착 체크
        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            AfterMove();
        }
    }

    private void Rotate()
    {
        // 타겟 방향 투사체 회전
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    protected virtual void AfterMove()
    {
        // 적 데미지 주는 로직 추가
        Destroy(gameObject);
    }

    protected virtual void SetAnimationClip()
    {
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        animatorOverride.GetOverrides(overrides);
        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].Key.name == "Flying")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, flyingAnim);
        }
    }
}
