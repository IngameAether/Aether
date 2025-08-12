using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoTTower : Tower
{
    [Header("DoT Tower Settings")]
    public GameObject projectilePrefab;
    public ProjectileConfig projectileConfig;

    protected override void Attack()
    {
        if (currentTarget == null) return;

        Vector2 aimPoint = currentTarget.position;
        GameObject go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<ProjectileController>();
        if (proj == null) { Debug.LogError("ProjectilePrefab에 ProjectileController가 없습니다."); Destroy(go); return; }

        // Homing으로 설정: target 전달 (proj.Init 내부의 mover가 유도 동작 수행)
        proj.Init(projectileConfig, currentTarget, aimPoint);
    }
}
