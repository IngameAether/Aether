using System.Collections.Generic;
using UnityEngine;

// 표의 한 줄(Row)에 해당하는 데이터를 담는 구조체
[System.Serializable]
public struct RarityByWave
{
    public int waveThreshold; // 이 확률이 적용되기 시작하는 웨이브 (예: 0, 10, 20...)
    public int normalChance;  // 노멀 등급 등장 확률
    public int rareChance;    // 레어 등급 등장 확률
    public int epicChance;    // 에픽 등급 등장 확률
}

[CreateAssetMenu(fileName = "MagicBookRarityTable", menuName = "TowerDefense/MagicBook Rarity Table")]
public class MagicBookRarityTable : ScriptableObject
{
    public List<RarityByWave> raritySettings;
}
