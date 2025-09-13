using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerReinforce : MonoBehaviour
{
    private Tower tower;

    private void Awake()
    {
        tower = GetComponent<Tower>();
    }

    /// <summary>
    /// 타워 강화 타입 지정: 처음 1번만 사용
    /// </summary>
    public void AssignReinforceType(ReinforceType type)
    {
        if (tower.reinforceType == ReinforceType.None)
        {
            tower.reinforceType = type;
        }
    }

    /// <summary>
    /// 타워 강화를 '요청'하는 함수. 실제 로직은 Tower.cs에 위임합니다.
    /// </summary>
    public void ReinforceTower()
    {
        if (tower == null) return;

        tower.Reinforce(tower.reinforceType);

        // 강화 성공 후 UI 갱신, 사운드/이펙트 재생 등의 코드는 여기에 남겨둘 수 있습니다.
        UpdateReinforceUI();
    }

    // 강화 레벨에 따른 마법진 변경이나 스탯 UI 갱신 등은 여기에 남겨둘 수 있습니다.
    private void UpdateReinforceUI()
    {
        Debug.Log("강화 UI를 갱신합니다.");
    }
}
