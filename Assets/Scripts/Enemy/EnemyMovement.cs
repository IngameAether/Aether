using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적의 경로 이동을 담당하며, 상태 이상(둔화, 기절, 공포) 효과를 실제 움직임에 반영합니다.
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    [Header("이동 경로 설정")]
    public List<Vector3> waypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;

    // 컴포넌트 참조
    private NormalEnemy normalEnemyStats;

    // 상태 이상 연동을 위한 변수
    private float speedMultiplier = 1.0f; // 이동 속도 배율 (둔화 효과용)
    private bool isStunned = false;       // 기절 상태 여부
    private bool isFeared = false;        // 공포 상태 여부

    // 이벤트
    public static event Action OnReachEndPoint;
    public static event Action OnEnemyDestroyed;

    private void Awake()
    {
        normalEnemyStats = GetComponent<NormalEnemy>();
        if (normalEnemyStats == null)
        {
            Debug.LogError("EnemyMovement: NormalEnemy 컴포넌트를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        // 기절 또는 공포 상태일 때는 모든 이동 로직을 정지
        if (isStunned || isFeared)
        {
            return;
        }

        // 필수 정보가 없으면 이동 로직을 실행하지 않음
        if (normalEnemyStats == null || waypoints == null || waypoints.Count == 0)
        {
            return;
        }

        // 둔화 효과가 적용된 최종 이동 속도 계산
        float finalMoveSpeed = normalEnemyStats.moveSpeed * speedMultiplier;

        // 경로 이동 로직
        if (currentWaypointIndex < waypoints.Count)
        {
            Vector3 targetPosition = waypoints[currentWaypointIndex];
            float step = finalMoveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                currentWaypointIndex++;
            }
        }
        else
        {
            // 경로 끝에 도달
            ReachedEndOfPath();
        }
    }

    #region Public API for EnemyStatusManager
    // --- 상태 이상 관리 시스템(EnemyStatusManager)을 위한 Public API ---

    public void ChangeSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void ResetSpeedMultiplier()
    {
        speedMultiplier = 1.0f;
    }

    public void SetStun(bool stunned)
    {
        isStunned = stunned;
    }

    public void ApplyFear(Vector3 sourcePosition, float duration)
    {
        if (!isFeared)
        {
            StartCoroutine(FearCoroutine(sourcePosition, duration));
        }
    }

    public void RemoveFear()
    {
        isFeared = false;
    }
    #endregion

    private IEnumerator FearCoroutine(Vector3 sourcePosition, float duration)
    {
        isFeared = true;
        Vector3 fleeDirection = (transform.position - sourcePosition).normalized;
        fleeDirection.z = 0; // 2D 게임 축에 맞게 조정

        float finalMoveSpeed = normalEnemyStats.moveSpeed * speedMultiplier;
        float timer = 0f;

        while (timer < duration && isFeared) // isFeared 플래그를 통해 외부에서 중단 가능
        {
            transform.Translate(fleeDirection * finalMoveSpeed * Time.deltaTime, Space.World);
            timer += Time.deltaTime;
            yield return null;
        }

        isFeared = false;
    }

    // 경로 설정 (외부 SpawnManager에서 호출)
    public void SetPath(List<Vector3> pathPoints)
    {
        if (pathPoints != null && pathPoints.Count > 0)
        {
            waypoints = pathPoints;
            currentWaypointIndex = 0;
            transform.position = waypoints[0];
        }
    }

    // 경로 이탈 및 사망 처리
    private void ReachedEndOfPath()
    {
        // GameManager.Instance?.LoseLife();
        OnReachEndPoint?.Invoke();
        Destroy(gameObject);
    }

    public void Die()
    {
        OnEnemyDestroyed?.Invoke();
        Destroy(gameObject);
    }
}
