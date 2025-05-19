using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("�� ���� ��ġ ����")]
    // ������ �� ������
    public GameObject enemyPrefab;
    [Header("���� ��ġ �� ��� ����")]
    // ���� ó�� ������ ��ġ (SetInitialPosition �޼ҵ忡�� ���)
    public Transform spawnPoint;
    // ���� ���� �̵��� ��� (watpoints ���)
    public List<Vector3> enemyPath = new List<Vector3>();

    [Header("���� Ÿ�̹� ����")]
    // ���� ���۱��� ��� �ð�
    public float startDelay = 1f;
    // �� ���� ����
    public float spawnInterval = 1f;
    // ������ ���� �� ������ = 3;
    public int numberOfEnemiesToSpawn = 3;
    // ������� ������ �� ��
    private int enemiesSpawned = 0;

    private void Start()
    {
        // Inspector���� ��ΰ� ������� ��� ��� �޽����� ���
        if (enemyPath == null || enemyPath.Count == 0)
        {
            Debug.LogWarning("SpawnManager: �� �̵� ���(enemyPath)�� �������� �ʾҽ��ϴ�!");
        }
        // �ڷ�ƾ�� ����Ͽ� ���� �������� �� ���� ����
        StartCoroutine(SpawnEnmiesRoutine());
    }

    IEnumerator SpawnEnmiesRoutine()
    {
        // ���� ���� �ð� ���
        yield return new WaitForSeconds(startDelay);

        while (enemiesSpawned < numberOfEnemiesToSpawn)
        {
            SpawnSingleEnemy(); // �� �ϳ� ����
            enemiesSpawned++;   // ������ �� �� ����
            // ��� �� �� �������� ���
            if (enemiesSpawned >= numberOfEnemiesToSpawn)
            {
                Debug.Log("��� �� ���� �Ϸ�");
                yield break; // ���� ����
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnSingleEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("������ �� �������� �����ؾ���");
            return;
        }

        Vector3 initialSpawnPosition;
        if (spawnPoint != null)
        {
            // spawnPoint Transform�� �����Ǿ� ������ �ش� ��ġ�� ���
            initialSpawnPosition = spawnPoint.position;
            Debug.Log($"SpawnManager: �� #{enemiesSpawned + 1}�� ������ ���� ����({initialSpawnPosition})�� ����.");
        }
        else
        {
            // spawnPoint�� �������� �ʾ����� SpawnManager ������Ʈ ��ü�� ��ġ�� ���
            initialSpawnPosition = transform.position;
            Debug.Log($"SpawnManager: �� #{enemiesSpawned + 1}�� SpawnManager ��ġ({initialSpawnPosition})�� ����.");
        }

        // �� ������ �ν��Ͻ� ����
        GameObject newEnemy = Instantiate(enemyPrefab, initialSpawnPosition, Quaternion.identity);
        // EnemyMovement ��ũ��Ʈ ��������
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            enemyMovement.SetPath(enemyPath);
            Debug.Log($"SpawnManager: �� #{enemiesSpawned + 1}�� �̵� ��� ���� �Ϸ�.");
        }
        else
        {
            Debug.LogWarning("�� �����տ� EnemyMovement ��ũ��Ʈ�� ����");
        }
    }
}
