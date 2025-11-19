using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("적 데이터")]
    [SerializeField] private EnemyData[] allEnemyData;
    private Dictionary<string, EnemyData> _enemyDataMap;

    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs;
    public MapManage mapManage;

    [Header("Wave Data")]
    [SerializeField] private bool loadFromCSV = true;
    [HideInInspector] public List<Wave> waveDatas;
    private WaveManager _waveManager;
    private Dictionary<string, int> _enemyIdToIndexMap;

    public static event Action OnAllEnemiesCleared;

    private int _aliveEnemies;
    private bool _isSpawningWave;
    private int spawnCount = 0;

    List<Vector3> _path;
    public static int _aliveS3Enemies = 0;

    private void Start()
    {
        BuildEnemyDataMap();
        BuildEnemyIdToIndexMap();

        if (loadFromCSV)
        {
            LoadWavesFromCSV();
        }

        Debug.Log("SpawnManager: 이벤트 구독 시작");

        EnemyMovement.OnEnemyDestroyed += HandleEnemyDestroyed;
        EnemyMovement.OnReachEndPoint += HandleEnemyDestroyed;

        _waveManager = FindObjectOfType<WaveManager>();
    }

    private void OnDestroy()
    {
        Debug.Log("SpawnManager: 이벤트 구독 해제");

        EnemyMovement.OnEnemyDestroyed -= HandleEnemyDestroyed;
        EnemyMovement.OnReachEndPoint -= HandleEnemyDestroyed;
    }

    /// <summary>
    /// 적 데이터 맵 구성
    /// </summary>
    private void BuildEnemyDataMap()
    {
        _enemyDataMap = new Dictionary<string, EnemyData>();
        foreach (var data in allEnemyData)
        {
            _enemyDataMap.TryAdd(data.ID, data);
        }
    }

    /// <summary>
    /// 적 ID를 enemyPrefabIndex로 매핑하는 딕셔너리 구성
    /// allEnemyData 배열의 순서대로 매핑됨
    /// </summary>
    private void BuildEnemyIdToIndexMap()
    {
        _enemyIdToIndexMap = new Dictionary<string, int>();

        for (int i = 0; i < allEnemyData.Length; i++)
        {
            string enemyId = allEnemyData[i].ID;
            _enemyIdToIndexMap[enemyId] = i;
        }
    }

    /// <summary>
    /// CSV 파일에서 Wave 데이터 로드
    /// </summary>
    private void LoadWavesFromCSV()
    {
        if (_enemyIdToIndexMap == null || _enemyIdToIndexMap.Count == 0)
        {
            return;
        }

        waveDatas = WaveDatabase.GetAllWaves(_enemyIdToIndexMap);
    }

    public IEnumerator SpawnWaveEnemies(Wave wave)
    {
        _isSpawningWave = true;

        List<Vector3> path = mapManage.GetPathWorldPositions();
        if (path == null || path.Count == 0)
        {
            Debug.LogError("SpawnManager: 경로가 없습니다.");
            _isSpawningWave = false;
            if (_aliveEnemies <= 0) OnAllEnemiesCleared?.Invoke();
        }

        // 마지막 방향 타일 하나 추가
        if (path.Count >= 2)
        {
            Vector3 dir = (path[^1] - path[^2]).normalized;
            path.Add(path[^1] + dir);
        }
        _path = path;

        if (wave.enemies == null || wave.enemies.Count == 0)
        {
            Debug.LogWarning("웨이브에 적 데이터가 없습니다");
            _isSpawningWave = false;
            if (_aliveEnemies <= 0) OnAllEnemiesCleared?.Invoke(); // 빈 웨이브 처리
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

        _isSpawningWave = false;
        if (_aliveEnemies <= 0)
        {
            Debug.Log("SpawnManager: Spawning complete and no alive enemies -> 이벤트 호출");
            OnAllEnemiesCleared?.Invoke();
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
        // 먼저 생성된 적 렌더링 순서 높게
        newEnemy.GetComponent<SpriteRenderer>().sortingOrder = 1000 - spawnCount;
        spawnCount++;
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        NormalEnemy enemy = newEnemy.GetComponent<NormalEnemy>();

        // 적 생성 시 Enemy Data 세팅
        EnemyInfoData info = EnemyDatabase.GetEnemyInfoData(enemy.GetEnemyId);
        int currentWave = _waveManager?.CurrentWaveLevel+1 ?? 0;
        enemy.Initialize(info, currentWave);

        string enemyId = enemy.GetEnemyId;

        if (enemyId == "S3") _aliveS3Enemies++;

        // 적 데이터 검색 후 적용
        EnemyData enemyData = null;
        if (!string.IsNullOrEmpty(enemyId) && _enemyDataMap.TryGetValue(enemyId, out var data))
        {
            enemyData = data;
        }
        enemy.SetEnemyData(enemyData, enemyIndex);


        if (enemyMovement != null)
        {
            // 특수 능력 체크
            if (enemyData != null && enemyData.HasAbility<BypassPath>())
            {
                Vector3 start = path[0];
                Vector3 end = path[^1];
                enemyMovement.SetStraightPath(start, end);
            }
            else
            {
                //enemyMovement.SetInitialPosition(initialSpawnPosition);
                enemyMovement.SetPath(path);
            }
        }
        else
        {
            Debug.LogError("SpawnManager: EnemyMovement 컴포넌트가 없습니다.");
        }

        _aliveEnemies++;
    }

    // B3: 2마리로 분할
    public void SpawnSplitEnemy(Vector3 initialSpawnPosition, int enemyIndex = 0)
    {
        GameObject newEnemy = Instantiate(enemyPrefabs[enemyIndex], initialSpawnPosition, Quaternion.identity);
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();
        NormalEnemy enemy = newEnemy.GetComponent<NormalEnemy>();
        string enemyId = enemy.GetEnemyId;

        // 적 데이터 검색 후 적용
        EnemyData enemyData = null;
        if (!string.IsNullOrEmpty(enemyId) && _enemyDataMap.TryGetValue(enemyId, out var data))
        {
            enemyData = data;
        }
        enemy.SetEnemyData(enemyData, enemyIndex);

        if (enemyMovement != null) enemyMovement.SetPath(_path);

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
