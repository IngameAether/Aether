using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("�� ���� ����")]
    // ������ �� ������
    public GameObject[] enemyPrefabs;
    public GameObject enemyPrefab;
    public MapManage mapManage; // MapManage ��ũ��Ʈ ���۷���

    [Header("���̺� ����")]
    public List<Wave> waves;
    public float initialDelay = 1f; // ù ���̺� ���� �� ��� �ð�
    public float waveBreakTime = 10f; // ���̺� �� ��� �ð�

    // ������� ������ �� ��
    private int enemiesSpawnedInCurrentWave = 0;
    // ������ ���� ���� ������ Ȯ���ϴ� �÷���
    private bool isSpawning = false;
    private static int currentWaveLevel = 0;

    private void Start()
    {
        // Inspector���� �����հ� MapManage ���۷����� ����Ǿ����� Ȯ��
        if (enemyPrefab == null)
        {
            Debug.LogError("SpawnManager: ������ �� �������� �������� �ʾҽ��ϴ�!");
            return;
        }
        if (mapManage == null)
        {
            Debug.LogError("SpawnManager: MapManage ���۷����� �������� �ʾҽ��ϴ�!");
            return;
        }
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("SpawnManager: ���̺� ������ ����ֽ��ϴ�!");
            return;
        }
        Debug.Log("SpawnManager �ʱ�ȭ �Ϸ�. Start ��ư Ŭ�� ��� ��.");
        StartCoroutine(SpawnEnmiesRoutine());
    }

    IEnumerator SpawnEnmiesRoutine()
    {
        isSpawning = true; // ���� ���� �÷��� ����

        // MapManage�κ��� ������ ��� �����͸� ������
        List<Vector3> path = mapManage.GetPathWorldPositions();
        if (path == null || path.Count == 0)
        {
            Debug.LogError("SpawnManager: ��ȿ�� ��� �����͸� ������ �� �����ϴ�. �� ���� �ߴ�.");
            isSpawning = false;
            yield break; // ��ΰ� ������ �ڷ�ƾ ����
        }

        // ���� ������ Ÿ���� ����� �ϱ� ���Ͽ� ������ �������� Ÿ�� �� ĭ �� �߰�
        if (path.Count >= 2)
        {
            Vector3 dir = (path[^1] - path[^2]).normalized;
            path.Add(path[^1] + dir);
        }

        // ���� ���� �ð� ���
        yield return new WaitForSeconds(initialDelay);

        for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
        {
            currentWaveLevel = waveIndex;
            Wave currentWave = waves[waveIndex];
            Debug.Log($"--- ���̺� {waveIndex + 1} ����! (�� {currentWave.numberOfEnemies}����) ---");

            enemiesSpawnedInCurrentWave = 0; // ���� ���̺� ������ �� �� �ʱ�ȭ

            while (enemiesSpawnedInCurrentWave < currentWave.numberOfEnemies)
            {
                Vector3 spawnPosition = path[0]; // ����� ù ��° ������ ���� ��ġ�� ���
                SpawnSingleEnemy(spawnPosition, path);
                enemiesSpawnedInCurrentWave++;  

                // ���� ���̺��� ��� ���� �����ϱ� �������� ���� ���� ���
                if (enemiesSpawnedInCurrentWave < currentWave.numberOfEnemies)
                {
                    yield return new WaitForSeconds(currentWave.spawnInterval);
                }
            }

            Debug.Log($"--- ���̺� {waveIndex + 1} �� ���� �Ϸ�. ---");

            // ������ ���̺갡 �ƴ϶�� ���̺� �� ��� �ð� ����
            if (waveIndex < waves.Count - 1)
            {
                Debug.Log($"���� ���̺���� {waveBreakTime}�� ���...");
                yield return new WaitForSeconds(waveBreakTime);
            }
            else
            {
                Debug.Log("��� ���̺� �Ϸ�!");
            }
        }

        isSpawning = false; // ��� ���̺� ���� ���� �÷��� ����
    }

    void SpawnSingleEnemy(Vector3 initialSpawnPosition, List<Vector3> path)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("������ �� �������� �����ؾ���");
            return;
        }

        GameObject newEnemy = Instantiate(enemyPrefabs[currentWaveLevel], initialSpawnPosition, Quaternion.identity);
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            enemyMovement.SetInitialPosition(initialSpawnPosition);
            enemyMovement.SetPath(path);
        }
        else
        {
            Debug.LogError("SpawnManager: ������ �� �����տ� EnemyMovement ������Ʈ�� �����ϴ�.");
        }
    }
}

[System.Serializable] 
public class Wave
{
    public int numberOfEnemies; // �� ���̺꿡�� ������ ���� ��
    public float spawnInterval; // �� ���̺꿡�� �� ���� ����
}