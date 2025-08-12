using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolicTower : Tower
{
    [Header("Parabolic Tower Settings")]
    public GameObject projectilePrefab;
    public ProjectileConfig projectileConfig;

    protected override void Attack()
    {
        if (currentTarget == null) return;

        // 발사 시점에 포착된 적의 위치를 고정 목표로 사용
        Vector2 aimPoint = currentTarget.position;

        GameObject go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<ProjectileController>();
        if (proj == null) { Debug.LogError("ProjectilePrefab에 ProjectileController가 없습니다."); Destroy(go); return; }

        // Parabolic은 비유도(aimPoint 고정)로 Init
        proj.Init(projectileConfig, null, aimPoint);
    }
}
