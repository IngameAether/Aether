using UnityEngine;
using TMPro; 

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI saveTimeText;

    public void UpdateUI(SaveSlot info)
    {
        // 데이터가 없는 슬롯 (새로운 게임)일 경우
        if (info == null || info.isEmpty)
        {
            waveText.text = "새로운 게임 시작";
            saveTimeText.text = "";
        }
        // 데이터가 있는 슬롯일 경우
        else
        {
            // SaveSlotInfo에 포함된 정보를 사용합니다.
            waveText.text = "Wave: " + info.currentWave;
            saveTimeText.text = "저장 시간: " + info.lastModified.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
