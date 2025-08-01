using System;
using UnityEngine;
using System.Collections.Generic;

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
    public string currentMapId;
    public List<TowerSaveData> towers = new();
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

[Serializable]
public class TowerSaveData
{
    public string towerId;
    public Vector3 position;
    public int rank;
    public ElementType elementType;
}
