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
    public int currentMapSeed;
    public int aetherAmount;
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
    public List<TowerSaveInfo> towers;
    public List<ElementSaveInfo> elements; // 배치된 원소 목록
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
    public int level; // 일반 강화 레벨 (reinforceLevel)
    public int lightReinforceCount; // Light 타입 강화 횟수
    public int darkReinforceCount; // Dark 타입 강화 횟수
    public int tileX; // 타워가 위치한 타일의 X 좌표
    public int tileY; // 타워가 위치한 타일의 Y 좌표
}

/// <summary>
/// 개별 원소의 정보를 저장하는 데이터 구조
/// </summary>
[Serializable]
public struct ElementSaveInfo
{
    public int elementType; // ElementType enum을 int로 저장 (Fire=0, Water=1, Earth=2, Air=3)
    public int tileX; // 원소가 위치한 타일의 X 좌표
    public int tileY; // 원소가 위치한 타일의 Y 좌표
}
