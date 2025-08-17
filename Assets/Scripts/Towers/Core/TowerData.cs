using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "TowerDefense/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("타워 기본 정보")]
    public string Name;
    public string ID;
    public int Level;
    public ElementType ElementType;
    public int MaxReinforce;

    [Header("조합 정보")]
    public List<string> RequiredTowerIds = new List<string>(); // 조합에 필요한 타워 ID들

    [Header("스탯 정보")]
    public float BaseDamage;
    public float BaseRange;
    public float BaseAttackSpeed;
    public float BaseCriticalRate;
}
