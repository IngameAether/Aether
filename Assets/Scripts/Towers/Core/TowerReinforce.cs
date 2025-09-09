using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerReinforce : MonoBehaviour
{
    protected Tower tower;
    protected TowerInformation TowerInformation;
    protected TowerSpriteController towerSpriteController;
    // protected TowerData towerData;

    private void Start()
    {
        towerSpriteController = GetComponent<TowerSpriteController>();
        tower = GetComponent<Tower>();
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
        if (TowerInformation.Rank == 4)
        {
            Debug.Log("Level 4 Tower can't reinforce!");
            return;
        }

        int bonusReinforce = ResourceManager.Instance.MaxElementUpgrade;
        if (TowerInformation.reinforceLevel >= TowerInformation.MaxReinforce + bonusReinforce)
        {
            Debug.Log("Tower reinforce level is too high!");
            return;
        }

        if (tower.reinforceType != ReinforceType.None)
        {
            TowerInformation.reinforceLevel++;
            TowerReinforceUpgrade();
            CheckLevelUpgrade();
        }
    }

    /// <summary>
    /// 타워 레벨업 조건 체크
    /// </summary>
    private void CheckLevelUpgrade()
    {
        if (TowerInformation.Rank == 2 && TowerInformation.reinforceLevel == 10)
        {
            TowerInformation.Rank++;
            TowerLevelUpgrade();
            Debug.Log($"{gameObject.name} lv.3으로 레벨업");
        }

        if (TowerInformation.Rank == 3 && TowerInformation.reinforceLevel == 20)
        {
            // 추가적인 능력 부여
        }
    }

    /// <summary>
    /// 타워 레벨 상승
    /// </summary>
    public void TowerLevelUpgrade()
    {
        TowerInformation.Rank++;
        if (towerSpriteController != null) towerSpriteController.SetSpritesByLevel(TowerInformation.Rank);
        SetTowerStat();
    }

    /// <summary>
    /// 타워 강화
    /// </summary>
    public void TowerReinforceUpgrade()
    {
        // 강화 레벨이 5,10,15,20이 되면 마법진 변화
        if (TowerInformation.reinforceLevel % 5 == 0)
        {
            if (towerSpriteController != null) towerSpriteController.SetSpriteByReinForce(TowerInformation.reinforceLevel);
        }
        SetTowerStat();
    }

    /// <summary>
    /// 레벨별 공격력, 속도, 치명타 조정
    /// </summary>
    protected void SetTowerStat()
    {
        TowerInformation.Damage = tower.towerData.GetDamage(GetReinforceLevel());
        TowerInformation.AttackSpeed = tower.towerData.GetAttackSpeed(GetReinforceLevel());
        TowerInformation.CriticalHit = tower.towerData.GetCriticalRate(GetReinforceLevel());
    }

    /// <summary>
    /// 강화 레벨 읽기
    /// </summary>
    /// <returns></returns>
    public int GetReinforceLevel() => TowerInformation.reinforceLevel;
}
