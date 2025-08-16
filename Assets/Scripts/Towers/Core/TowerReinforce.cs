using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerReinforce : MonoBehaviour
{
    protected Tower tower;
    protected TowerSetting towerSetting;
    protected TowerSpriteController towerSpriteController;

    private void Start()
    {
        towerSpriteController = GetComponent<TowerSpriteController>();
        tower = GetComponent<Tower>();
        towerSetting = tower.GetTowerSetting();
    }

    /// <summary>
    /// 타워 강화 타입 지정: 처음 1번만 사용
    /// </summary>
    /// <param name="type"></param>
    public void AssignReinforceType(ReinforceType type)
    {
        if (tower.reinforceType == ReinforceType.None)
        {
            tower.reinforceType = type;
        }
    }

    /// <summary>
    /// 타워 강화
    /// </summary>
    public void ReinforceTower()
    {
        if (towerSetting.Rank == 4)
        {
            Debug.Log("Level 4 Tower can't reinforce!");
            return;
        }

        int bonusReinforce = ResourceManager.Instance.MaxElementUpgrade;
        if (towerSetting.reinforceLevel >= towerSetting.MaxReinforce + bonusReinforce)
        {
            Debug.Log("Tower reinforce level is too high!");
            return;
        }

        if (tower.reinforceType != ReinforceType.None)
        {
            towerSetting.reinforceLevel++;
            TowerReinforceUpgrade();
            CheckLevelUpgrade();
        }
    }

    /// <summary>
    /// 타워 레벨업 조건 체크
    /// </summary>
    private void CheckLevelUpgrade()
    {
        if (towerSetting.Rank == 2 && towerSetting.reinforceLevel == 10)
        {
            towerSetting.Rank++;
            TowerLevelUpgrade();
            Debug.Log($"{gameObject.name} lv.3으로 레벨업");
        }

        if (towerSetting.Rank == 3 && towerSetting.reinforceLevel == 20)
        {
            // 추가적인 능력 부여
        }
    }

    /// <summary>
    /// 타워 레벨 상승
    /// </summary>
    public void TowerLevelUpgrade()
    {
        towerSetting.Rank++;
        if (towerSpriteController != null) towerSpriteController.SetSpritesByLevel(towerSetting.Rank);
    }

    /// <summary>
    /// 타워 강화
    /// </summary>
    public void TowerReinforceUpgrade()
    {
        // 강화 레벨이 5,10,15,20이 되면 마법진 변화
        if (towerSetting.reinforceLevel % 5 == 0)
        {
            if (towerSpriteController != null) towerSpriteController.SetSpriteByReinForce(towerSetting.reinforceLevel);
        }
    }

    /// <summary>
    /// 강화 레벨 읽기
    /// </summary>
    /// <returns></returns>
    public int GetReinforceLevel() => towerSetting.reinforceLevel;
}
