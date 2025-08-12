using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolicMover : IProjectileMover
{
    ProjectileConfig cfg;
    ProjectileController owner;
    Vector2 startPos;
    Vector2 targetPos;
    Vector2 horDir;
    float horSpeed;
    float vertSpeed;
    float gravity;
    float timeElapsed;
    float travelTime;
    Transform spriteChild; // 시각적 오프셋을 위한 child transform

    public ParabolicMover(ProjectileConfig config, SpriteRenderer spriteRendererChild)
    {
        cfg = config;
        spriteChild = spriteRendererChild != null ? spriteRendererChild.transform : null;
        gravity = cfg.gravity;
    }

    public void Init(ProjectileController _owner, Vector2 aimPoint, Transform target = null)
    {
        owner = _owner;
        startPos = owner.transform.position;
        targetPos = aimPoint;
        Vector2 delta = targetPos - startPos;
        float angleRad = Mathf.Deg2Rad * 45f;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        float speed = cfg.speed;

        // horizontal direction on plane (top-down): normalized vector from start to target
        if (delta.magnitude < 0.001f) horDir = Vector2.right;
        else horDir = delta.normalized;

        horSpeed = speed * cos;
        vertSpeed = speed * sin;

        float horDist = delta.magnitude;
        travelTime = horDist / Mathf.Max(horSpeed, 0.0001f);
        timeElapsed = 0f;
    }

    public void Tick(float deltaTime)
    {
        timeElapsed += deltaTime;

        // ground position movement
        Vector2 groundPos = startPos + horDir * (horSpeed * timeElapsed);
        owner.transform.position = new Vector3(groundPos.x, groundPos.y, owner.transform.position.z);

        // vertical height for visual
        float h = vertSpeed * timeElapsed - 0.5f * gravity * timeElapsed * timeElapsed;
        if (h < 0f) h = 0f;

        if (spriteChild != null)
        {
            // spriteChild는 로컬 Y 축을 높이 표현으로 사용
            spriteChild.localPosition = new Vector3(0f, h, 0f);
        }
        else
        {
            // child가 없으면 약간 world Y 오프셋 (주의: collider도 영향 받음)
            owner.transform.position = new Vector3(groundPos.x, groundPos.y + h, owner.transform.position.z);
        }

        // 충돌 감지(도달 또는 근접)
        if (timeElapsed >= travelTime - 0.02f || Vector2.Distance(groundPos, targetPos) < 0.15f)
        {
            // 임팩트 처리
            owner.ApplyImpactAt(groundPos);

            // 이 예제에서는 폭발형/장판 등의 처리는 ProjectileController.ApplyImpactAt에서 처리
            owner.ReturnToPool();
        }
    }
}
