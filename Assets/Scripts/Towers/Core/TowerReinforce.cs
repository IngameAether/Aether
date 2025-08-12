using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerReinforce : MonoBehaviour
{
    protected Tower tower;
    protected TowerSetting towerSetting;

    protected TowerSpriteController towerSpriteController;
    public enum ReinforceType { None, Light, Dark };

    [Header("Tower Enhance")]
    [SerializeField] private ReinforceType curReinforceType = ReinforceType.None;

    private void Start()
    {
        towerSpriteController = GetComponent<TowerSpriteController>();
        tower = GetComponent<Tower>();
        towerSetting = tower.GetTowerSetting();
    }

    // 타워 강화 타입 지정: 처음 1번만 사용
    public void AssignReinforceType(ReinforceType type)
    {
        if (curReinforceType == ReinforceType.None)
            curReinforceType = type;
    }

    // 타워 강화
    public void ReinforceTower()
    {
        if (towerSetting.rank == 4)
        {
            Debug.Log("Level 4 Tower can't reinforce!");
            return;
        }

        if (curReinforceType != ReinforceType.None)
        {
            towerSetting.reinforceLevel++;
            TowerReinforceUpgrade();
            CheckLevelUpgrade();
        }
    }

    // 타워 레벨업 조건 체크
    private void CheckLevelUpgrade()
    {
        if (towerSetting.rank == 2 && towerSetting.reinforceLevel == 10)
        {
            towerSetting.rank++;
            TowerLevelUpgrade();
            Debug.Log($"{gameObject.name} lv.3으로 레벨업");
        }

        if (towerSetting.rank == 3 && towerSetting.reinforceLevel == 20)
        {
            // 추가적인 능력 부여
        }
    }

    // 타워 레벨 상승
    public void TowerLevelUpgrade()
    {
        towerSetting.rank++;
        if (towerSpriteController != null) towerSpriteController.SetSpritesByLevel(towerSetting.rank);
    }

    // 타워 강화
    public void TowerReinforceUpgrade()
    {
        // 강화 레벨이 5,10,15,20이 되면 마법진 변화
        if (towerSetting.reinforceLevel % 5 == 0)
        {
            if (towerSpriteController != null) towerSpriteController.SetSpriteByReinForce(towerSetting.reinforceLevel);
        }
    }

    // 강화 레벨 읽기
    public int GetReinforceLevel() => towerSetting.reinforceLevel;
    // 강화 타입 읽기
    public ReinforceType GetReinforceType() => curReinforceType;
}
