using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("�̵� ��� ����")]
    // �̵��� ��� ���� ���
    public List<Vector3> waypoints = new List<Vector3>();
    // �̵� �ӵ�
    private int currentWaypointIndex = 0;
    private NormalEnemy normalEnemyStats; // NormalEnemy �ν��Ͻ�
    private float currentMoveSpeed; // Ư�� �̵� �ӵ�

    public static event Action OnReachEndPoint;

    private void Awake()
    {
        // NormalEnemy ������Ʈ ��������
        normalEnemyStats = GetComponent<NormalEnemy>();
        if (normalEnemyStats == null)
        {
            Debug.LogError("NormalEnemy ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    // �ʱ� ��ġ�� �����ϴ� �޼ҵ�
    public void SetInitialPosition(Vector3 initialPosition)
    {
        transform.position = initialPosition;
    }

    // ���(wayPoints)�� �����ϴ� �޼ҵ�(�ܺο��� ȣ��, SpawnManager���� ���)
    public void SetPath(List<Vector3> pathPoints)
    {
        if (pathPoints != null && pathPoints.Count > 0)
        {
            waypoints = pathPoints;
            // ��� ���ۺ��� �̵�
            currentWaypointIndex = 0; 
        }
        else
        {
            Debug.LogWarning("EnemyMovement: ��ȿ�� ��� ������ �ʿ�");
        }
    }

    private void Update()
    {
        // NormalEnemyStats�� ���ų� ��ΰ� ������ �̵����� ����
        if (normalEnemyStats == null || waypoints == null || waypoints.Count == 0)
        {
            return; 
        }

        // Ư�� �̵� �ӵ� ����
        currentMoveSpeed = normalEnemyStats.moveSpeed;

        // �̵��� ��������Ʈ�� �ִ��� Ȯ��
        if (currentWaypointIndex < waypoints.Count)
        {
            // ���� ��ǥ ��������Ʈ ��ġ
            Vector3 targetPosition = waypoints[currentWaypointIndex];
            // ��ǥ ��ġ�� �̵�
            float step = currentMoveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            // ��ǥ ��ġ�� ���� �����ߴ��� Ȯ��
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                // ���� ��������Ʈ�� �̵� ��ǥ ����
                currentWaypointIndex++;
            }
        }
        else
        {
            // ��� �̵��� �Ϸ�ƴٸ�
            ReachedEndOfPath();
        }
    }
    // ����� ���� �������� �� ȣ��Ǵ� �޼ҵ�
    void ReachedEndOfPath()
    {
        Debug.Log("���� ��ǥ ������ ����");
        Destroy(gameObject);
        OnReachEndPoint?.Invoke();
    }
}