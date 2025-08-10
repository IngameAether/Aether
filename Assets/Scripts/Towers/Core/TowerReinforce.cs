using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerReinforce : MonoBehaviour
{
    protected TowerSpriteController towerSpriteController;
    public enum ReinforceType { None, Light, Dark };

    [Header("Tower Enhance")]
    private ReinforceType curReinforceType = ReinforceType.None;
    private int reinforceLevel = 0;

    [Header("Tower Level")]
    [SerializeField] private int level = 1;
    [SerializeField] private int reinforce = 0;

    private void Start()
    {
        towerSpriteController = GetComponent<TowerSpriteController>();
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
        if (level == 4)
        {
            Debug.Log("Level 4 Tower can't reinforce!");
            return;
        }

        if (curReinforceType != ReinforceType.None)
        {
            reinforceLevel++;
            TowerReinforceUpgrade();
            CheckLevelUpgrade();
        }
    }

    // 타워 레벨업 조건 체크
    private void CheckLevelUpgrade()
    {
        if (level == 2 && reinforceLevel == 10)
        {
            level++;
            TowerLevelUpgrade();
            Debug.Log($"{gameObject.name} lv.3으로 레벨업");
        }

        if (level == 3 && reinforceLevel == 20)
        {
            // 추가적인 능력 부여
        }
    }

    // 타워 레벨 상승
    public virtual void TowerLevelUpgrade()
    {
        level++;
        if (towerSpriteController != null) towerSpriteController.SetSpritesByLevel(level);
    }

    // 타워 강화
    public virtual void TowerReinforceUpgrade()
    {
        reinforce++;

        // 강화 레벨이 5,10,15,20이 되면 마법진 변화
        if (reinforce % 5 == 0)
        {
            if (towerSpriteController != null) towerSpriteController.SetSpriteByReinForce(reinforce);
        }
    }

    // 강화 레벨 읽기
    public int GetReinforceLevel() => reinforceLevel;
    // 강화 타입 읽기
    public ReinforceType GetReinforceType() => curReinforceType;
}
