using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

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
    SaveSlotSelect
}

public class MainMenuUI : MonoBehaviour
{
    [Header("UI 패널")]
    public GameObject levelSelectPanel;
    public GameObject saveSlotSelectPanel;
    public GameObject blurBackgroundImage;

    [Header("레벨 선택 UI")]
    public TMP_Text levelNameText;
    public Image waveImageDisplay;
    public Button leftArrowButton;
    public Button rightArrowButton;
    public Button playLevelButton;

    [Header("저장 슬롯 선택 UI")] 
    public Button saveSlotLeftArrow;
    public Button saveSlotRightArrow;
    public Button selectSlotButton; // 슬롯 선택(플레이) 버튼
    public SaveSlotUI[] saveSlotUIs; // 슬롯 3개의 UI 스크립트 배열
    public TMP_Text slotIndicatorText;
    private int currentSaveSlotIndex = 0;

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

        // 저장된 게임
        if (saveSlotLeftArrow != null)
            saveSlotLeftArrow.onClick.AddListener(OnSaveSlotLeftArrowClick);
        if (saveSlotRightArrow != null)
            saveSlotRightArrow.onClick.AddListener(OnSaveSlotRightArrowClick);
        if (selectSlotButton != null)
            selectSlotButton.onClick.AddListener(OnSelectSlotButtonClick);
    }

    // 특정 패널만 활성화하고 다른 패널은 비활성화하는 함수
    async void ShowPanel(UIPanelState targetPanel)
    {
        // 모든 패널 비활성화
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (saveSlotSelectPanel != null) saveSlotSelectPanel.SetActive(false);

        // 블러 배경 이미지 활성화/비활성화
        bool needsBlurBackground = (targetPanel == UIPanelState.SaveSlotSelect || targetPanel == UIPanelState.LevelSelect);
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
            case UIPanelState.SaveSlotSelect: 
                if (saveSlotSelectPanel != null)
                {
                    saveSlotSelectPanel.SetActive(true);
                    await UpdateAllSaveSlotsUI(); // <--- 비동기로 슬롯 정보 업데이트 함수 호출
                    UpdateCarouselVisuals();      // <--- 캐러셀 시각 효과 업데이트 함수 호출
                }
                break;
        }

        // 현재 상태 업데이트
        currentPanelState = targetPanel;
    }

    #region 버튼 클릭 이벤트 핸들러
    // --- 버튼 클릭 이벤트 핸들러 함수들 ---

    public void OnStartButtonClick()
    {
        ShowPanel(UIPanelState.SaveSlotSelect);
    }

    // '설정' 버튼 클릭 시 (가장 큰 변화)
    public void OnSettingsButtonClick()
    {
        // PopUpManager를 통해 "Settings" 팝업을 열도록 요청
        PopUpManager.Instance.OpenPopUpMainMenu("Settings");
    }

    public void OnPlayLevelButtonClick()
    {
        // 현재 선택된 웨이브로 게임 시작 (저장 파일 불러오지 않음)
        StartGame(currentSelectedWaveIndex, false);
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

    // 저장 슬롯 선택 패널의 왼쪽 화살표 클릭
    void OnSaveSlotLeftArrowClick()
    {
        currentSaveSlotIndex--;
        if (currentSaveSlotIndex < 0)
        {
            currentSaveSlotIndex = saveSlotUIs.Length - 1;
        }
        UpdateCarouselVisuals();
    }

    // 저장 슬롯 선택 패널의 오른쪽 화살표 클릭
    void OnSaveSlotRightArrowClick()
    {
        currentSaveSlotIndex++;
        if (currentSaveSlotIndex >= saveSlotUIs.Length)
        {
            currentSaveSlotIndex = 0;
        }
        UpdateCarouselVisuals();
    }

    // "이 슬롯으로 플레이" 버튼 클릭
    public async void OnSelectSlotButtonClick()
    {
        // GameSaveManager에 현재 선택한 슬롯의 인덱스를 저장합니다.
        GameSaveManager.Instance.SelectedSlotIndex = currentSaveSlotIndex;

        // 선택한 슬롯의 데이터를 불러오거나, 비어있으면 null
        GameSaveData data = await GameSaveManager.Instance.LoadGameAsync(currentSaveSlotIndex);

        ShowPanel(UIPanelState.LevelSelect);
    }

    public void OnBackButtonClick()
    {
        switch (currentPanelState)
        {
            case UIPanelState.SaveSlotSelect: 
                ShowPanel(UIPanelState.MainMenu);
                break;
            case UIPanelState.LevelSelect:
                ShowPanel(UIPanelState.SaveSlotSelect);
                break;
            default:
                Debug.LogWarning("현재 패널에서 '뒤로가기' 동작이 정의되지 않았습니다: " + currentPanelState);
                break;
        }
    }

    // 모든 저장 슬롯 UI의 정보를 업데이트
    async Task UpdateAllSaveSlotsUI()
    {
        for (int i = 0; i < saveSlotUIs.Length; i++)
        {
            GameSaveData data = await GameSaveManager.Instance.LoadGameAsync(i);
            saveSlotUIs[i].UpdateUI(data);
        }
    }

    // 캐러셀의 시각적 표현을 업데이트 (크기 조절로 선택된 슬롯 강조)
    void UpdateCarouselVisuals()
    {
        // 슬롯 번호 텍스트 업데이트 (1부터 시작하도록 +1)
        if (slotIndicatorText != null)
        {
            slotIndicatorText.text = $"Slot {currentSaveSlotIndex + 1}";
        }

        // 기존의 크기 조절 로직은 그대로 둡니다.
        for (int i = 0; i < saveSlotUIs.Length; i++)
        {
            if (i == currentSaveSlotIndex)
            {
                saveSlotUIs[i].transform.localScale = Vector3.one * 1.2f;
            }
            else
            {
                saveSlotUIs[i].transform.localScale = Vector3.one;
            }
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
