using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeTower : Tower
{
    [Header("Cone Tower Settings")]
    public ConeAttack coneAttack; // 타워 자식으로 ConeAttack 컴포넌트 연결
    public float coneDamage = 15f;

    protected override void Attack()
    {
        if (currentTarget == null) return;

        Vector2 origin = transform.position;
        Vector2 forward = (currentTarget.position - transform.position).normalized;
        if (forward.magnitude < 0.001f) forward = Vector2.right;

        // coneAttack.Fire 내부에서 지정된 range/angle/damage를 사용하므로 여기서는 damage 설정 전달 또는 coneAttack의 필드 직접 설정
        coneAttack.damage = coneDamage;
        coneAttack.Fire(origin, forward);
    }
}
