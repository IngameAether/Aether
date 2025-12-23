using System.Collections.Generic;
using UnityEngine;

public class Fly : FireObjectBase
{
    [Space]
    [Header("Flying Variables")]
    [SerializeField] protected AnimationClip flyingAnim;
    [SerializeField] private float speed;
    [SerializeField] private float flyingAnimSpeed;

    public override void Init(Vector2 tower, Transform target, float damage)
    {
        base.Init(tower, target, damage);

        // 방향 정하기
        Rotate();

        // flying 애니메이션 재생
        PlayFlyingAnim(flyingAnimSpeed);
    }

    protected virtual void Update()
    {
        Move();

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            // 현재 가장 영향력이 큰(첫 번째) 클립의 이름을 출력
            string currentClipName = clipInfo[0].clip.name;
            Debug.Log("현재 재생 중인 클립: " + currentClipName);
        }
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
        OnTargetHit();
    }

    protected override void SetAnimationClip()
    {
        base.SetAnimationClip();

        if (flyingAnim != null) animatorOverride["L1F_flying"] = flyingAnim;
    }
}
