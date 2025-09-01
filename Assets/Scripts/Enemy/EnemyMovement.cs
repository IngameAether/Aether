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

    public List<Vector3> points = new List<Vector3>();  // 입구, 출구
    public bool bypassPath = false;  // 경로 무시 플래그

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

        // 경로 무시 플래그 값에 따라 경로 지정
        List<Vector3> targetPoints = bypassPath ? points : waypoints;

        // 필요한 참조가 없거나 경로가 비어있으면 이동하지 않음
        if (normalEnemyStats == null || targetPoints == null || targetPoints.Count == 0)
        {
            return;
        }

        // 둔화 효과가 적용된 최종 이동 속도 계산
        float finalMoveSpeed = normalEnemyStats.moveSpeed * speedMultiplier;

        // 경로 이동 로직
        if (currentWaypointIndex < targetPoints.Count)
        {
            Vector3 targetPosition = targetPoints[currentWaypointIndex];
            float step = finalMoveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            bool isLastWayPoint = currentWaypointIndex == targetPoints.Count - 1;
            // 다음 목표와 얼만큼 도달했는지 체크하는 거리 조정
            float distWayPoint = isLastWayPoint ? 0.5f : 0.05f;

            // 목표 위치에 거의 도달했는지 확인 (충분히 가까워지면 다음 웨이포인트로 이동)
            if (Vector3.Distance(transform.position, targetPosition) < distWayPoint)
            {
                currentWaypointIndex++;
            }
        }
        else
        {
            // 적이 마지막 타일 바깥을 빠져나가 없어지게 함
            if (Vector3.Distance(transform.position, targetPoints[^1]) < 0.5f) ReachedEndOfPath();
        }
    }

    // 특수 적 S2 능력-경로 무시, 직선 이동
    public void SetStraightPath(Vector3 start, Vector3 end)
    {
        points.Clear();
        points.Add(start);
        points.Add(end);
        currentWaypointIndex = 0;
        transform.position = start;
        bypassPath = true;
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
}
