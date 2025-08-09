using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerReinforce : MonoBehaviour
{
    private Tower tower;
    public enum ReinforceType { None, Light, Dark };

    [Header("Tower Enhance")]
    private ReinforceType curReinforceType = ReinforceType.None;
    private int reinforceLevel = 0;

    [Header("Tower Level")]
    private int towerLevel = 1;

    // 타워 강화 타입 지정: 처음 1번만 사용
    public void AssignReinforceType(ReinforceType type)
    {
        if (curReinforceType == ReinforceType.None)
            curReinforceType = type;
    }

    // 타워 강화
    public void ReinforceTower()
    {
        if (towerLevel == 4)
        {
            Debug.Log("Level 4 Tower can't reinforce!");
            return;
        }

        if (curReinforceType != ReinforceType.None)
        {
            reinforceLevel++;
            tower.TowerReinforceUpgrade();
            CheckLevelUpgrade();
        }
    }

    // 타워 레벨업 조건 체크
    private void CheckLevelUpgrade()
    {
        if (towerLevel == 2 && reinforceLevel == 10)
        {
            towerLevel++;
            tower.TowerLevelUpgrade();
            Debug.Log($"{gameObject.name} lv.3으로 레벨업");
        }

        if (towerLevel == 3 && reinforceLevel == 20)
        {
            // 추가적인 능력 부여
        }
    }

    // 강화 레벨 읽기
    public int GetReinforceLevel() => reinforceLevel;
}
