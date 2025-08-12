using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightTower : Tower
{
    [Header("Straight Tower Settings")]
    public GameObject projectilePrefab; // ProjectileController가 붙은 프리팹
    public ProjectileConfig projectileConfig;

    protected override void Attack()
    {
        if (currentTarget == null) return;

        Vector2 aimPoint = currentTarget.position;

        // Instantiate 대신 풀 사용 권장
        GameObject go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<ProjectileController>();
        if (proj == null)
        {
            Debug.LogError("ProjectilePrefab에 ProjectileController가 없습니다.");
            Destroy(go);
            return;
        }

        // 유도형이면 target 전달, 비유도면 target null(혹은 aimPoint 전달)
        if (projectileConfig.isHoming) proj.Init(projectileConfig, currentTarget, aimPoint);
        else proj.Init(projectileConfig, null, aimPoint);
    }
}
