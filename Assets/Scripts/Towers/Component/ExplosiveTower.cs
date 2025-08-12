using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveTower : Tower
{
    [Header("Explosive Tower Settings")]
    public GameObject projectilePrefab;
    public ProjectileConfig projectileConfig;

    protected override void Attack()
    {
        if (currentTarget == null) return;

        Vector2 aimPoint = currentTarget.position;

        GameObject go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<ProjectileController>();
        if (proj == null) { Debug.LogError("ProjectilePrefab에 ProjectileController가 없습니다."); Destroy(go); return; }

        // Explosive은 비유도 직선 이동으로 aimPoint에 도달하면 폭발
        proj.Init(projectileConfig, null, aimPoint);
    }
}
