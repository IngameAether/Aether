using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs;
    public MapManage mapManage;

    [Header("Wave Data")]
    public List<Wave> waves;

    public static event Action OnAllEnemiesCleared;

    private int _aliveEnemies;

    private void Start()
    {
        Debug.Log("SpawnManager: 이벤트 구독 시작");

        EnemyMovement.OnEnemyDestroyed += HandleEnemyDestroyed;
        EnemyMovement.OnReachEndPoint += HandleEnemyDestroyed;
    }

    private void OnDestroy()
    {
        Debug.Log("SpawnManager: 이벤트 구독 해제");

        EnemyMovement.OnEnemyDestroyed -= HandleEnemyDestroyed;
        EnemyMovement.OnReachEndPoint -= HandleEnemyDestroyed;
    }

    public IEnumerator SpawnWaveEnemies(Wave wave)
    {
        List<Vector3> path = mapManage.GetPathWorldPositions();
        if (path == null || path.Count == 0)
        {
            Debug.LogError("SpawnManager: 경로가 없습니다.");
            yield break;
        }

        // 마지막 방향 타일 하나 추가
        if (path.Count >= 2)
        {
            Vector3 dir = (path[^1] - path[^2]).normalized;
            path.Add(path[^1] + dir);
        }

        if (wave.enemies == null || wave.enemies.Count == 0)
        {
            Debug.LogWarning("웨이브에 적 데이터가 없습니다");
            yield break;
        }

        float waveStartTime = Time.time;
        float maxWaveEndTime = 0f;
        var nextSpawnTimes = new Dictionary<int, float>();

        foreach (var enemy in wave.enemies)
        {
            nextSpawnTimes[enemy.enemyPrefabIndex] = waveStartTime + enemy.startTime;
            maxWaveEndTime = Mathf.Max(maxWaveEndTime, enemy.endTime);
        }

        float waveEndTime = waveStartTime + maxWaveEndTime;
        while (Time.time < waveEndTime)
        {
            float currentTime = Time.time;
            foreach (var enemy in wave.enemies)
            {
                float enemyEndTime = waveStartTime + enemy.endTime;
                if (currentTime >= nextSpawnTimes[enemy.enemyPrefabIndex] && currentTime < enemyEndTime)
                {
                    SpawnSingleEnemy(path[0], path, enemy.enemyPrefabIndex);
                    nextSpawnTimes[enemy.enemyPrefabIndex] = currentTime + enemy.spawnInterval;
                }
            }

            yield return null;
        }
    }

    private void SpawnSingleEnemy(Vector3 initialSpawnPosition, List<Vector3> path, int enemyIndex = 0)
    {
        if (enemyPrefabs == null || enemyIndex >= enemyPrefabs.Length)
        {
            Debug.LogError("잘못된 enemyIndex로 스폰 시도");
            return;
        }

        GameObject newEnemy = Instantiate(enemyPrefabs[enemyIndex], initialSpawnPosition, Quaternion.identity);
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            enemyMovement.SetPath(path);
        }
        else
        {
            Debug.LogError("SpawnManager: EnemyMovement 컴포넌트가 없습니다.");
        }

        _aliveEnemies++;
    }

    private void HandleEnemyDestroyed()
    {
        _aliveEnemies--;
        Debug.Log($"SpawnManager: Enemy destroyed. Alive: {_aliveEnemies}");
        if (_aliveEnemies <= 0)
        {
            Debug.Log("SpawnManager: All enemies cleared! -> 이벤트 호출");
            OnAllEnemiesCleared?.Invoke();
        }
    }

    [System.Serializable]
    public class WaveEnemyData
    {
        public int enemyPrefabIndex;
        public float spawnInterval;
        public float startTime;
        public float endTime;
    }

    [System.Serializable]
    public class Wave
    {
        public List<WaveEnemyData> enemies = new List<WaveEnemyData>();
    }
}
