using UnityEngine;
using TMPro; 

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI saveTimeText;
    public TextMeshProUGUI aetherText;
    // 게임에 맞게 필요한 다른 UI 요소들을 여기에 추가 (예: 재화, 플레이어 정보 등)

    // GameSaveData를 받아서 UI 내용을 업데이트하는 함수
    public void UpdateUI(GameSaveData data)
    {
        // 데이터가 없는 슬롯 (새로운 게임)일 경우
        if (data == null)
        {
            waveText.text = "새로운 게임 시작"; // 또는 "비어있는 슬롯"
            saveTimeText.text = "";
            aetherText.text = "";
            // TODO: 다른 UI 요소들도 비워줍니다.
        }
        // 데이터가 있는 슬롯일 경우
        else
        {
            waveText.text = "Wave: " + data.currentWave;
            saveTimeText.text = "저장 시간: " + data.lastSaveTime.ToString("yyyy-MM-dd HH:mm");

            // 재화(Resource) 정보가 null이 아닌지 확인 후 표시
            if (data.resources != null)
            {
                aetherText.text = "에테르: " + data.resources.aether.ToString();
            }
            else
            {
                aetherText.text = "에테르: 0";
            }

            // TODO: 다른 데이터들도 여기에 채워줍니다.
        }
    }
}
