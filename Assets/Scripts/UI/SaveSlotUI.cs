using UnityEngine;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI saveTimeText;

    public void UpdateUI(SaveSlot info)
    {
        if (waveText != null)
            waveText.gameObject.SetActive(false);

        if (saveTimeText != null)
            saveTimeText.gameObject.SetActive(false);
    }
}
