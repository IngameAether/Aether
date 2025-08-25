using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Wave 데이터를 관리할 구조체 정의
[System.Serializable]
public struct WaveData
{
    public string waveName; // 예: "1 Wave", "2 Wave"
    public Sprite waveImage; // 해당 Wave에 표시될 그림 (Sprite)
    public string sceneName; // 해당 Wave에 연결된 씬 이름
}

// UI 패널 상태를 나타내는 열거형 (Settings 제거)
public enum UIPanelState
{
    None,
    MainMenu,
    LevelSelect,
    LoadSave
}

public class MainMenuUI : MonoBehaviour
{
    [Header("UI 패널")]
    public GameObject levelSelectPanel;
    public GameObject loadSavePanel;
    public GameObject blurBackgroundImage;

    [Header("레벨 선택 UI")]
    public TMP_Text levelNameText;
    public Image waveImageDisplay;
    public Button leftArrowButton;
    public Button rightArrowButton;
    public Button playLevelButton;

    [Header("레벨 데이터")]
    public WaveData[] waveDatas;
    private int currentSelectedWaveIndex = 0;

    // 현재 활성화된 패널 상태 추적
    private UIPanelState currentPanelState = UIPanelState.None;

    private void Awake()
    {
        // 게임 시작 시 메인 메뉴 패널만 보이도록 설정
        ShowPanel(UIPanelState.MainMenu);

        // 버튼 이벤트 리스너 연결
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(OnLeftArrowButtonClick);
        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(OnRightArrowButtonClick);
        if (playLevelButton != null)
            playLevelButton.onClick.AddListener(OnPlayLevelButtonClick);
    }

    // 특정 패널만 활성화하고 다른 패널은 비활성화하는 함수
    void ShowPanel(UIPanelState targetPanel)
    {
        // 모든 패널 비활성화
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (loadSavePanel != null) loadSavePanel.SetActive(false);

        // 블러 배경 이미지 활성화/비활성화
        bool needsBlurBackground = (targetPanel == UIPanelState.LoadSave || targetPanel == UIPanelState.LevelSelect);
        if (blurBackgroundImage != null)
        {
            blurBackgroundImage.SetActive(needsBlurBackground);
        }

        // 목표 패널 활성화
        switch (targetPanel)
        {
            case UIPanelState.LevelSelect:
                if (levelSelectPanel != null)
                {
                    levelSelectPanel.SetActive(true);
                    UpdateLevelDisplay();
                }
                break;
            case UIPanelState.LoadSave:
                if (loadSavePanel != null) loadSavePanel.SetActive(true);
                break;
        }

        // 현재 상태 업데이트
        currentPanelState = targetPanel;
    }

    #region 버튼 클릭 이벤트 핸들러
    // --- 버튼 클릭 이벤트 핸들러 함수들 ---

    public void OnStartButtonClick()
    {
        ShowPanel(UIPanelState.LevelSelect);
    }

    // '설정' 버튼 클릭 시 (가장 큰 변화)
    public void OnSettingsButtonClick()
    {
        // PopUpManager를 통해 "Settings" 팝업을 열도록 요청
        PopUpManager.Instance.OpenPopUp("Settings");
    }

    public void OnPlayLevelButtonClick()
    {
        ShowPanel(UIPanelState.LoadSave);
    }

    public void OnLeftArrowButtonClick()
    {
        currentSelectedWaveIndex--;
        if (currentSelectedWaveIndex < 0)
        {
            currentSelectedWaveIndex = waveDatas.Length - 1;
        }
        UpdateLevelDisplay();
    }

    public void OnRightArrowButtonClick()
    {
        currentSelectedWaveIndex++;
        if (currentSelectedWaveIndex >= waveDatas.Length)
        {
            currentSelectedWaveIndex = 0;
        }
        UpdateLevelDisplay();
    }

    public void OnNewGameButtonClick()
    {
        StartGame(currentSelectedWaveIndex, false);
    }

    public void OnLoadGameButtonClick()
    {
        if (waveDatas.Length > 0)
        {
            bool saveFileExists = CheckSaveFile(currentSelectedWaveIndex);
            StartGame(currentSelectedWaveIndex, saveFileExists);
        }
        else
        {
            Debug.LogError("Wave 데이터가 설정되지 않았습니다!");
            ShowPanel(UIPanelState.MainMenu);
        }
    }

    public void OnBackButtonClick()
    {
        switch (currentPanelState)
        {
            case UIPanelState.LevelSelect:
                ShowPanel(UIPanelState.MainMenu);
                break;
            case UIPanelState.LoadSave:
                ShowPanel(UIPanelState.LevelSelect);
                break;
            default:
                Debug.LogWarning("현재 패널에서 '뒤로가기' 동작이 정의되지 않았습니다: " + currentPanelState);
                break;
        }
    }

    public void OnQuitGameButtonClick()
    {
        Debug.Log("게임 종료");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    void StartGame(int waveIndex, bool loadSave)
    {
        string sceneToLoad = waveDatas[waveIndex].sceneName;

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.TransitionToScene(sceneToLoad);
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
            }
        }
        else
        {
            Debug.LogError($"Wave {waveIndex + 1}에 대한 씬 이름이 설정되지 않았습니다.");
        }
    }

    void UpdateLevelDisplay()
    {
        if (waveDatas != null && waveDatas.Length > 0)
        {
            if (levelNameText != null)
            {
                levelNameText.text = waveDatas[currentSelectedWaveIndex].waveName;
            }
            if (waveImageDisplay != null)
            {
                waveImageDisplay.sprite = waveDatas[currentSelectedWaveIndex].waveImage;
            }
        }
        else
        {
            Debug.LogWarning("WaveDatas 배열이 비어 있습니다.");
        }
    }

    bool CheckSaveFile(int waveIndex)
    {
        // TODO: 실제 세이브 파일 존재 여부 확인 로직 구현
        return false;
    }
}
