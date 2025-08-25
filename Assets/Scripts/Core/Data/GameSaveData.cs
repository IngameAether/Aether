using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class BuffSaveEntry
{
    public ElementType elementType; // ElementDamage이면 해당 element, else None
    public float value; // 퍼센트값(정수로 저장되지만 float로 보관해도 무방)
}

[Serializable]
public class GameSaveData
{
    public int saveSlot;
    public DateTime lastSaveTime = DateTime.Now;
    public string gameVersion = "1.0.0";

    // 게임 진행 상황
    public int currentWave;
    public int playerLife;

    // 재화 시스템
    public ResourceData resources = new();

    // 맵 및 타워 정보
    public int currentMapSeed;
    public List<TowerSetting> towers = new();

    // 버프관련 정보
    public List<BuffSaveEntry> activeBuffs = new List<BuffSaveEntry>();
}

[Serializable]
public class ResourceData
{
    public int aether;
    public int lightElement;
    public int darkElement;

    public ResourceData() { }
    public ResourceData(int aether, int light, int dark)
    {
        this.aether = aether;
        this.lightElement = light;
        this.darkElement = dark;
    }
}
