using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyDatabase
{
    private static Dictionary<string, EnemyInfoData> _enemyDict;

    public static void LoadData()
    {
        _enemyDict = new Dictionary<string, EnemyInfoData>();
        var data = CSVReader.Read("Enemy_Data");
        foreach (var row in data)
        {
            EnemyInfoData enemyInfo = new EnemyInfoData(row);
            _enemyDict[enemyInfo.IdCode] = enemyInfo;
        }

    }

    public static EnemyInfoData GetEnemyInfoData(string idCode)
    {
        if (_enemyDict == null) LoadData();

        if (_enemyDict.TryGetValue(idCode, out var enemyInfo)) return enemyInfo;

        else
        {
            Debug.Log($"{idCode} 데이터 없음");
            return null;
        }
    }
}
