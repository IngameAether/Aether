using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메인 메뉴 UI에 표시될 슬롯의 '요약' 정보
/// </summary>
[Serializable]
public class SaveSlot
{
    public bool isEmpty;
    public DateTime lastModified;
    public int currentWave;
}
/// <summary>
/// 게임의 모든 저장 데이터를 담는 최상위 클래스
/// </summary>
[Serializable]
public class GameSaveDataInfo
{
    public int saveSlot;
    public string gameVersion;
    public int currentWave;
    public int playerLife;
    public int currentMapSeed;
    public ResourceData resources;
    public List<TowerSetting> towers;
    public Dictionary<string, int> ownedMagicBooks; // 획득한 마법책 목록 (코드, 스택)
}


/// <summary>
/// 재화 정보를 저장하는 데이터 구조
/// </summary>
[Serializable]
public struct ResourceDataInfo
{
    public int coin;
    public int lightElement;
    public int darkElement;

    public ResourceDataInfo(int c, int l, int d)
    {
        coin = c;
        lightElement = l;
        darkElement = d;
    }
}

/// <summary>
/// 개별 타워의 정보를 저장하는 데이터 구조
/// </summary>
[Serializable]
public struct TowerSaveInfo
{
    public string towerId; // 타워 종류를 식별할 ID (예: "fire_tower_tier1")
    public int level;
}
