using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneUIManager : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button settingsButton; // 게임 씬의 설정 버튼 (Inspector에서 연결)
    [SerializeField] private Button showBuffsButton;  // 현재 버프 목록을 보여주는 버튼 (Inspector에서 연결)
    [SerializeField] private Button showHelpButton; // 도움말 버튼 (Inspector에서 연결)

    void Start()
    {
        // 게임 씬이 시작될 때 이 코드가 실행됩니다.
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners(); // 혹시 모를 에디터 연결을 제거
            settingsButton.onClick.AddListener(OnSettingsButtonClicked); // 버튼에 코드 리스너 추가
        }
        else
        {
            Debug.LogWarning("settingsButton이 GameSceneUIManager에 연결되지 않았습니다.");
        }

        // 버프 목록 버튼 연결 
        if (showBuffsButton != null)
        {
            showBuffsButton.onClick.RemoveAllListeners();
            showBuffsButton.onClick.AddListener(OnShowBuffsButtonClicked);
        }
        else
        {
            Debug.LogWarning("showBuffsButton이 GameSceneUIManager에 연결되지 않았습니다.");
        }

        // 도움말 버튼 연결 
        if (showHelpButton != null)
        {
            showHelpButton.onClick.RemoveAllListeners();
            showHelpButton.onClick.AddListener(OnShowHelpButtonClicked);
        }
    }

    // 세팅 목록 버튼 클릭 시 호출
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

    // 버프 목록 버튼 클릭 시 호출
    private void OnShowBuffsButtonClicked()
    {
        if (PopUpManager.Instance != null)
        {
            // "OwnedBooks"는 PopUpManager에 등록한 버프 목록 팝업의 이름입니다.
            PopUpManager.Instance.OpenPopUpInGame("OwnedBooks");
        }
        else
        {
            Debug.LogError("PopUpManager.Instance가 게임 씬에서 null입니다. 심각한 오류!");
        }
    }

    // 도움말 버튼 클릭 시 호출
    private void OnShowHelpButtonClicked()
    {
        if (PopUpManager.Instance != null)
        {
            // PopUpManager에 등록한 이름("Help")을 사용합니다.
            PopUpManager.Instance.OpenPopUpInGame("Help");
        }
        else
        {
            Debug.LogError("PopUpManager.Instance가 게임 씬에서 null입니다!");
        }
    }
}
