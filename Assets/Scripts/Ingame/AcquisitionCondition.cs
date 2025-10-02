// 획득 조건의 종류를 정의합니다.
using System.Collections.Generic;
using UnityEngine;

public enum ConditionType
{
    BookCombination,    // 책 조합으로 획득
    BossKill,           // 특정 보스 처치 시 획득
    TowerCount          // 특정 타워 개수 달성 시 획득
}

// 조건 데이터를 담을 범용 클래스. [System.Serializable]을 붙여 인스펙터에 표시되게 합니다.
[System.Serializable]
public class AcquisitionCondition
{
    public ConditionType type;

    [Tooltip("타입이 BookCombination일 때만 사용: 필요한 책 코드들")]
    public List<string> requiredBookCodes;

    [Tooltip("타입이 BossKill일 때만 사용: 몇 번째 보스인지 (예: B2)")]
    public string bossIndex;

    [Tooltip("타입이 TowerCount일 때만 사용: 필요한 타워 개수 (예: 9)")]
    public int requiredTowerCount;
}
