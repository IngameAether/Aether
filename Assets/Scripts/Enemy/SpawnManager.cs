using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("�� ���� ����")]
    // ������ �� ������
    public GameObject enemyPrefab;
    public MapManage mapManage; // MapManage ��ũ��Ʈ ���۷���

    [Header("UI")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TMP_Text waveText;

    [Header("���̺� ����")]
    public List<Wave> waves;
    public float initialDelay = 1f; // ù ���̺� ���� �� ��� �ð�
    public float waveBreakTime = 10f; // ���̺� �� ��� �ð�
    
    // ������ ���� ���� ������ Ȯ���ϴ� �÷���
    private bool isSpawning = false;
    private float _currentEndTime;
    private int _reachedEndEnemyCount = 0;
    private Coroutine _spawnCoroutine;

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
        
        gameOverUI.SetActive(false);
        waveText.text = "0 wave";

        EnemyMovement.OnReachEndPoint += HandleReachedEndEnemy;
        
        Debug.Log("SpawnManager �ʱ�ȭ �Ϸ�. Start ��ư Ŭ�� ��� ��.");
        _spawnCoroutine = StartCoroutine(SpawnEnmiesRoutine());
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
        // ���� ���� �ð� ���
        yield return new WaitForSeconds(initialDelay);

        for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
        {
            waveText.text = $"{waveIndex + 1} wave";
            
            yield return new WaitForSeconds(waves[waveIndex].startTime);
            
            Wave currentWave = waves[waveIndex];
            Debug.Log($"--- ���̺� {waveIndex + 1} ����!---");
            
            _currentEndTime = Time.time + waves[waveIndex].endTime;

            while (Time.time <= _currentEndTime)
            {
                Vector3 spawnPosition = path[0]; // ����� ù ��° ������ ���� ��ġ�� ���
                SpawnSingleEnemy(spawnPosition, path);

                // ���� ���̺��� ��� ���� �����ϱ� �������� ���� ���� ���
                if (Time.time <= _currentEndTime)
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

        GameObject newEnemy = Instantiate(enemyPrefab, initialSpawnPosition, Quaternion.identity);
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

    private void GameOver()
    {
        // 게임 끝나는 기준이 없음. 적 5명 지나가면 끝나는걸로 설정
        StopCoroutine(_spawnCoroutine);
        gameOverUI.SetActive(true);
    }

    private void HandleReachedEndEnemy()
    {
        _reachedEndEnemyCount++;
        if (_reachedEndEnemyCount >= 5)
        {
            GameOver();
        }
    }
}

[System.Serializable] 
public class Wave
{
    public float spawnInterval;
    public float startTime;
    public float endTime;
}