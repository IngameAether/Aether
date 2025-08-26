using UnityEngine;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("슬롯 정보 표시")]
    public TMP_Text selectedSlotText; // 슬롯 번호를 표시할 텍스트 UI

    // 이 UI 패널이 활성화될 때마다 호출됩니다.
    private void OnEnable()
    {
        UpdateSlotInfo();
    }

    // 슬롯 정보를 업데이트하는 함수
    private void UpdateSlotInfo()
    {
        // GameSaveManager가 존재하고, 텍스트 UI가 연결되어 있는지 확인
        if (selectedSlotText != null && GameSaveManager.Instance != null)
        {
            // GameSaveManager에 저장된 슬롯 인덱스는 0부터 시작하므로,
            // 화면에 보여줄 때는 +1을 해줍니다.
            int slotNumber = GameSaveManager.Instance.SelectedSlotIndex + 1;

            // 슬롯 인덱스가 유효한 경우 (선택된 경우)
            if (slotNumber > 0)
            {
                selectedSlotText.text = $"Slot {slotNumber}";
            }
            else // 유효하지 않은 경우 (선택되지 않았거나 오류)
            {
                selectedSlotText.text = "Slot ?"; // 혹은 비워둡니다.
            }
        }
    }
}
