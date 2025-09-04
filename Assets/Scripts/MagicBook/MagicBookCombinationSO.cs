using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMagicBookCombination", menuName = "MagicBook/Combination")]
public class MagicBookCombinationSO : ScriptableObject
{
    // [Tooltip("이 조합을 완성하기 위해 필요한 매직북 코드 목록")]
    public List<string> requiredBookCodes;

    // [Tooltip("조합 완성 시 보상으로 주어질 매직북 코드")]
    public string rewardBookCode;

    // [Tooltip("게임 세션 중에 이 조합이 이미 완성되었는지 여부 (런타임 전용)")]
    [System.NonSerialized]
    public bool isCompleted = false;

    // 게임이 새로 시작될 때 isCompleted를 리셋하기 위한 메서드
    public void Reset()
    {
        isCompleted = false;
    }
}
