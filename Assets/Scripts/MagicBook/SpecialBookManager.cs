using System.Collections.Generic;
using UnityEngine;

public class SpecialBookManager : MonoBehaviour
{
    // 모든 Special 등급의 책들을 여기에 연결
    public List<MagicBookData> specialBooks;

    void Start()
    {
        // 보스가 죽었을 때, 타워가 건설되었을 때 등의 이벤트를 구독
        // 예: BossManager.OnBossDied += CheckBossKillCondition;
        // 예: TowerBuilder.OnTowerPlaced += CheckTowerCountCondition;
    }

    // 2번째 보스가 죽었을 때 호출될 함수 (예시)
    void CheckBossKillCondition(string bossIndex)
    {
        foreach (var book in specialBooks)
        {
            if (book.condition.type == ConditionType.BossKill && book.condition.bossIndex == bossIndex)
            {
                MagicBookManager.Instance.SelectBook(book.Code); // 책 지급
            }
        }
    }

    // 타워가 건설될 때마다 호출될 함수 (예시)
    void CheckTowerCountCondition()
    {
        // TODO: 모든 타워를 순회하며 같은 타워가 9개 이상 있는지 확인하는 로직
        // 조건이 만족되면 해당 책을 지급
    }
}
