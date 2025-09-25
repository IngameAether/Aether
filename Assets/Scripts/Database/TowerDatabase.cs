using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TowerDatabase
{
    private static Dictionary<string, TowerInfoData> _towerDict;

    public static void LoadData()
    {
        _towerDict = new Dictionary<string, TowerInfoData>();
        var data = CSVReader.Read("Tower_Data");
        foreach (var row in data)
        {
            TowerInfoData towerInfo = new TowerInfoData(row);
            _towerDict[towerInfo.IdCode] = towerInfo;
        }

    }

    public static TowerInfoData GetTowerInfoData(string idCode)
    {
        if (_towerDict == null) LoadData();

        if (_towerDict.TryGetValue(idCode, out var towerInfo)) return towerInfo;

        else
        {
            Debug.Log($"{idCode} 데이터 없음");
            return null;
        }
    }
}
