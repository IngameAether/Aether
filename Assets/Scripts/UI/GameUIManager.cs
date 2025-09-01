using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneUIManager : MonoBehaviour
{
    [SerializeField] private Button settingsButton; // 게임 씬의 설정 버튼 (Inspector에서 연결)

    void Start()
    {
        // 게임 씬이 시작될 때 이 코드가 실행됩니다.
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners(); // 혹시 모를 에디터 연결을 제거
            settingsButton.onClick.AddListener(OnSettingsButtonClicked); // 버튼에 코드 리스너 추가
        }
    }

    private void OnSettingsButtonClicked()
    {
        // 여기에서 PopUpManager.Instance에 접근합니다.
        if (PopUpManager.Instance != null)
        {
            PopUpManager.Instance.OpenPopUpInGame("Settings");
        }
        else
        {
            Debug.LogError("PopUpManager.Instance가 게임 씬에서 null입니다. 심각한 오류!");
        }
    }
}
