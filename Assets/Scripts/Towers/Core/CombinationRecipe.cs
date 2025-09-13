using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CombinationRecipe", menuName = "TowerDefense/Combination Recipe")]
public class CombinationRecipe : ScriptableObject
{
    [Header("조합에 필요한 타워 ID 리스트")]
    public List<string> requiredTowerIds;

    [Header("조합 결과로 나올 타워의 데이터")]
    public TowerData resultingTowerData;
}
