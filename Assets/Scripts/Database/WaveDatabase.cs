using System.Collections.Generic;
using UnityEngine;

public static class WaveDatabase
{
    private static Dictionary<int, SpawnManager.Wave> _waveDict;
    private static readonly SpawnManager.Wave s_Empty = new();

    public static void LoadData(string resourcePath = "Wave_Data", Dictionary<string, int> ResolveEnemyPrefabIndex = null)
    {
        if (ResolveEnemyPrefabIndex == null)
        {
            Debug.LogError("WaveDatabase.LoadData: ResolveEnemyPrefabIndex가 null입니다!");
            return;
        }

        _waveDict = new Dictionary<int, SpawnManager.Wave>();

        // CSV 읽기
        var rows = CSVReader.Read(resourcePath);
        if (rows == null || rows.Count == 0)
        {
            Debug.LogError("WaveData를 불러오지 못했습니다.");
            return;
        }

        Debug.Log($"WaveDatabase: CSV에서 {rows.Count}개의 행 로드됨");

        foreach (var row in rows)
        {
            // wave 인덱스 파싱
            if (!row.TryGetValue("wave", out var waveObj)) continue;

            int waveIndex;
            if (waveObj is int iVal) waveIndex = iVal;
            else if (waveObj is float fVal) waveIndex = Mathf.RoundToInt(fVal);
            else if (!int.TryParse(waveObj.ToString(), out waveIndex)) continue;

            var wave = new SpawnManager.Wave();
            var enemyDataCount = 0;

            // 모든 헤더(컬럼) 순회 (wave 제외)
            foreach (var kv in row)
            {
                var header = kv.Key;
                if (header == "wave") continue;

                var cellObj = kv.Value;
                if (!TryCoerceTo9Digits(cellObj, out var cell)) continue; // 9자리 복원

                if (!IsValid9Digit(cell))
                {
                    Debug.LogWarning($"Wave {waveIndex}, {header}의 값 '{cell}'이 유효한 9자리 형식이 아닙니다.");
                    continue;
                }

                Decode9Digit(cell, out var start, out var end, out var interval);

                if (!ResolveEnemyPrefabIndex.TryGetValue(header, out var enemyPrefabIndex))
                {
                    Debug.LogWarning(
                        $"Wave {waveIndex}: '{header}' 적 ID를 찾을 수 없습니다. 사용 가능한 키: {string.Join(", ", ResolveEnemyPrefabIndex.Keys)}");
                    continue;
                }

                wave.enemies.Add(new SpawnManager.WaveEnemyData
                {
                    enemyPrefabIndex = enemyPrefabIndex,
                    startTime = start,
                    endTime = end,
                    spawnInterval = interval
                });
                enemyDataCount++;
            }

            _waveDict[waveIndex] = wave;
        }
    }

    public static List<SpawnManager.Wave> GetAllWaves(Dictionary<string, int> ResolveEnemyPrefabIndex)
    {
        LoadData("Wave_Data", ResolveEnemyPrefabIndex);
        if (_waveDict == null || _waveDict.Count == 0)
        {
            Debug.LogError("WaveDatabase: 웨이브 데이터가 없습니다.");
            return new List<SpawnManager.Wave>();
        }

        var waveList = new List<SpawnManager.Wave>();
        var maxWaveIndex = 0;
        foreach (var key in _waveDict.Keys)
            if (key > maxWaveIndex)
                maxWaveIndex = key;

        for (var i = 1; i <= maxWaveIndex; i++)
            if (_waveDict.TryGetValue(i, out var wave))
                waveList.Add(wave);
            else
                waveList.Add(new SpawnManager.Wave()); // 빈 웨이브

        Debug.Log($"WaveDatabase: {waveList.Count}개의 웨이브 로드 완료");
        return waveList;
    }

    public static SpawnManager.Wave GetWave(int waveIndex)
    {
        if (_waveDict == null)
        {
            Debug.LogError("WaveDatabase.GetWave: _waveDict가 null입니다. GetAllWaves()를 먼저 호출해야 합니다.");
            return s_Empty;
        }

        if (_waveDict.TryGetValue(waveIndex, out var wave)) return wave;
        Debug.LogWarning($"Wave {waveIndex} 데이터가 없습니다.");
        return s_Empty;
    }

// 숫자형으로 들어온 셀을 9자리 문자열로 복원
    private static bool TryCoerceTo9Digits(object cellObj, out string cell)
    {
        cell = null;
        if (cellObj == null) return false;

        switch (cellObj)
        {
            case string s:
                s = s.Trim();
                if (string.IsNullOrEmpty(s)) return false;
                if (!IsAllDigits(s)) return false;
                if (s.Length == 9)
                {
                    cell = s;
                    return true;
                }

                if (s.Length < 9)
                {
                    cell = s.PadLeft(9, '0');
                    return true;
                } // 선행 0 복원

                return false;

            case int iv:
                cell = iv.ToString("D9"); // 선행 0 유지
                return true;

            case float fv:
                cell = Mathf.RoundToInt(fv).ToString("D9"); // 정수로 간주하여 9자리
                return true;

            default:
                var t = cellObj.ToString()?.Trim();
                if (string.IsNullOrEmpty(t) || !IsAllDigits(t)) return false;
                if (t.Length == 9)
                {
                    cell = t;
                    return true;
                }

                if (t.Length < 9)
                {
                    cell = t.PadLeft(9, '0');
                    return true;
                }

                return false;
        }
    }

    private static bool IsAllDigits(string s)
    {
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c < '0' || c > '9') return false;
        }

        return true;
    }

    private static bool IsValid9Digit(string s)
    {
        if (s.Length != 9) return false;
        return IsAllDigits(s);
    }

    // 010150010 → start=1.0, end=15.0, interval=1.0
    private static void Decode9Digit(string s, out float start, out float end, out float interval)
    {
        var startTimeRaw = int.Parse(s.Substring(0, 3));
        var endTimeRaw = int.Parse(s.Substring(3, 3));
        var spawnIntervalRaw = int.Parse(s.Substring(6, 3));
        start = startTimeRaw / 10f;
        end = endTimeRaw / 10f;
        interval = spawnIntervalRaw / 10f;
    }
}
