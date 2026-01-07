using System.Collections.Generic;
using UnityEngine;

public static class GameDataDatabase
{
    private static Dictionary<string, GameDataInfo> _gameDataDict;

    public static void LoadData()
    {
        _gameDataDict = new Dictionary<string, GameDataInfo>();
        var data = CSVReader.Read("Game_Data");

        foreach (var row in data)
        {
            string name = row["List"].ToString();
            if (!string.IsNullOrEmpty(name))
            {
                GameDataInfo gameDataInfo = new GameDataInfo(row);
                _gameDataDict[name] = gameDataInfo;
            }
        }

        Debug.Log($"GameDataDatabase: {_gameDataDict.Count}개의 게임 데이터 로드 완료");
    }

    public static GameDataInfo GetGameDataInfo(string name)
    {
        if (_gameDataDict == null) LoadData();

        if (_gameDataDict.TryGetValue(name, out var gameDataInfo))
        {
            return gameDataInfo;
        }
        else
        {
            Debug.LogWarning($"GameDataDatabase: {name} 데이터 없음");
            return null;
        }
    }

    public static int GetInt(string name, int defaultValue = 0)
    {
        var info = GetGameDataInfo(name);
        return info != null ? info.GetInt() : defaultValue;
    }

    public static float GetFloat(string name, float defaultValue = 0f)
    {
        var info = GetGameDataInfo(name);
        return info != null ? info.GetFloat() : defaultValue;
    }

    public static int[] GetIntArray(string name)
    {
        var info = GetGameDataInfo(name);
        return info != null ? info.GetIntArray() : new int[0];
    }

    public static float[] GetFloatArray(string name)
    {
        var info = GetGameDataInfo(name);
        return info != null ? info.GetFloatArray() : new float[0];
    }

    public static bool HasData(string name)
    {
        if (_gameDataDict == null) LoadData();
        return _gameDataDict.ContainsKey(name);
    }

    public static Dictionary<string, GameDataInfo> GetAllData()
    {
        if (_gameDataDict == null) LoadData();
        return _gameDataDict;
    }
}
