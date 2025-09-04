using System;
using UnityEngine;

public class MagicBookSelectionUI : MonoBehaviour
{
    [SerializeField] private MagicBookButton[] _bookButtons;
    private MagicBookData[] _currentChoices;

    // 팝업이 활성화될 때 자동으로 호출됩니다.
    private void OnEnable()
    {
        // MagicBookManager에게 미리 준비된 선택지를 요청합니다.
        // 일반, 보스, 조합 보상 등 모든 경우를 이 한 줄로 처리합니다.
        _currentChoices = MagicBookManager.Instance.GetPreparedSelection();
        UpdateUI();
    }

    // UI를 현재 선택지 데이터로 업데이트합니다.
    public void UpdateUI()
    {
        if (_currentChoices == null || _currentChoices.Length == 0)
        {
            Debug.LogError("표시할 책이 없습니다. MagicBookManager를 확인하세요.");
            return;
        }

        // 모든 버튼을 일단 끈 상태로 시작 (선택지가 1개일 경우를 대비)
        foreach (var button in _bookButtons)
        {
            button.gameObject.SetActive(false);
        }

        // 선택지 개수만큼 버튼을 켜고 데이터를 할당합니다.
        for (int i = 0; i < _currentChoices.Length; i++)
        {
            if (i < _bookButtons.Length)
            {
                _bookButtons[i].SetBookData(_currentChoices[i].Description);
                _bookButtons[i].gameObject.SetActive(true);
            }
        }
    }

    // 버튼의 OnClick() 이벤트에서 호출할 수 있도록 'public'으로 선언합니다.
    public void OnBookSelected(int buttonIndex)
    {
        if (_currentChoices == null || buttonIndex >= _currentChoices.Length) return;

        var bookData = _currentChoices[buttonIndex];
        MagicBookManager.Instance.SelectBook(bookData.Code);

        // 팝업 닫기는 PopUpManager에게 맡깁니다.
        PopUpManager.Instance.CloseCurrentPopUp();
    }
}
