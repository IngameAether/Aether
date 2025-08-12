using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType
{
    Straight,
    Homing,
    Parabolic
}

public enum HitType
{
    Direct,
    Explosive,
    GroundAoE
}

[CreateAssetMenu(menuName = "TD/ProjectileConfig")]
public class ProjectileConfig : ScriptableObject
{
    [Header("Movement")]
    public MovementType movementType = MovementType.Straight;
    public bool isHoming = false;
    public float turnSpeed = 180f; // deg/sec, for homing
    public float speed = 12f; // 발사 속도 (고정)
    public float gravity = 9.81f; // 포물선용 시각적 중력 (단위 임의)

    [Header("Hit")]
    public HitType hitType = HitType.Direct;
    public float damage = 20f;
    public float radius = 1.5f; // 폭발/장판 반경

    [Header("Life & Layers")]
    public float lifeTime = 5f;
    public LayerMask enemyLayer = 1 << 8; // 기본 Enemy 레이어 (인스펙터에서 맞춰주세요)

    [Header("Ground AoE (zone) - optional")]
    public bool createsZoneOnExplode = false; // 팀장 확인용: 코드에서 주석으로 남겨둠
    public float zoneDuration = 3f;
    public float zoneTickInterval = 0.5f;
    public float zoneTickDamage = 5f;

    [Header("Visual")]
    public bool useSpriteChildForArc = true; // Parabolic 시 child sprite로 높이 오프셋 적용
}
