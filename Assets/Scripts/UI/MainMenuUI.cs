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

// UI 패널 상태를 나타내는 열거형
public enum UIPanelState
{
    MainMenu,
    SaveSlotSelect,
    LevelSelect
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
    public Button selectSlotButton;
    public SaveSlotUI[] saveSlotUIs;
    public TMP_Text slotIndicatorText;
    private int currentSaveSlotIndex = 0;

    [Header("레벨 데이터")]
    public WaveData[] waveDatas;
    private int currentSelectedWaveIndex = 0;

    private UIPanelState currentPanelState = UIPanelState.MainMenu;

    private void Start()
    {
        // 게임 시작 시 메인 메뉴 패널만 보이도록 설정 (Awake 대신 Start 권장)
        ShowPanel(UIPanelState.MainMenu);

        // 버튼 이벤트 리스너 연결
        leftArrowButton?.onClick.AddListener(OnLeftArrowButtonClick);
        rightArrowButton?.onClick.AddListener(OnRightArrowButtonClick);
        playLevelButton?.onClick.AddListener(OnPlayLevelButtonClick);

        saveSlotLeftArrow?.onClick.AddListener(OnSaveSlotLeftArrowClick);
        saveSlotRightArrow?.onClick.AddListener(OnSaveSlotRightArrowClick);
        selectSlotButton?.onClick.AddListener(OnSelectSlotButtonClick);
    }

    // 특정 패널만 활성화하고 다른 패널은 비활성화하는 함수
    void ShowPanel(UIPanelState targetPanel)
    {
        currentPanelState = targetPanel;

        // 모든 패널 기본 비활성화
        levelSelectPanel?.SetActive(false);
        saveSlotSelectPanel?.SetActive(false);

        // 블러 배경은 필요한 패널에서만 활성화
        bool needsBlur = (targetPanel == UIPanelState.SaveSlotSelect || targetPanel == UIPanelState.LevelSelect);
        blurBackgroundImage?.SetActive(needsBlur);

        // 목표 패널만 활성화하고 필요한 UI 업데이트 실행
        switch (targetPanel)
        {
            case UIPanelState.LevelSelect:
                levelSelectPanel?.SetActive(true);
                UpdateLevelDisplay();
                break;
            case UIPanelState.SaveSlotSelect:
                saveSlotSelectPanel?.SetActive(true);
                UpdateAllSaveSlotsUI();
                UpdateCarouselVisuals();
                break;
        }
    }

    #region 버튼 클릭 이벤트 핸들러

    // [게임 시작] 버튼 -> 저장 슬롯 선택 패널로 이동
    public void OnStartButtonClick() => ShowPanel(UIPanelState.SaveSlotSelect);

    // [설정] 버튼 -> PopUpManager를 통해 설정 팝업 열기
    public void OnSettingsButtonClick() => PopUpManager.Instance.OpenPopUpMainMenu("Settings");

    // [나가기] 버튼 -> 게임 종료
    public void OnQuitGameButtonClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // [뒤로가기] 버튼 -> 현재 상태에 따라 이전 패널로 이동
    public void OnBackButtonClick()
    {
        if (currentPanelState == UIPanelState.SaveSlotSelect)
        {
            ShowPanel(UIPanelState.MainMenu);
        }
        else if (currentPanelState == UIPanelState.LevelSelect)
        {
            ShowPanel(UIPanelState.SaveSlotSelect);
        }
    }

    // --- 레벨 선택 패널 ---
    // [플레이] 버튼 -> '새 게임'으로 레벨 선택 씬 로드
    public void OnPlayLevelButtonClick()
    {
        // '새 게임'이므로 선택된 슬롯 인덱스를 -1 (없음)로 설정
        GameSaveManager.Instance.SelectedSlotIndex = -1;
        StartGame(currentSelectedWaveIndex);
    }
    public void OnLeftArrowButtonClick()
    {
        currentSelectedWaveIndex = (currentSelectedWaveIndex - 1 + waveDatas.Length) % waveDatas.Length;
        UpdateLevelDisplay();
    }
    public void OnRightArrowButtonClick()
    {
        currentSelectedWaveIndex = (currentSelectedWaveIndex + 1) % waveDatas.Length;
        UpdateLevelDisplay();
    }

    // --- 저장 슬롯 패널 ---
    // [이 슬롯으로 플레이] 버튼 -> '이어하기' 또는 '새 게임'으로 레벨 선택 패널로 이동
    public void OnSelectSlotButtonClick()
    {
        GameSaveManager.Instance.SelectedSlotIndex = currentSaveSlotIndex;

        ShowPanel(UIPanelState.LevelSelect);
    }

    void OnSaveSlotLeftArrowClick()
    {
        currentSaveSlotIndex = (currentSaveSlotIndex - 1 + saveSlotUIs.Length) % saveSlotUIs.Length;
        UpdateCarouselVisuals();
    }
    void OnSaveSlotRightArrowClick()
    {
        currentSaveSlotIndex = (currentSaveSlotIndex + 1) % saveSlotUIs.Length;
        UpdateCarouselVisuals();
    }
    #endregion

    #region UI 업데이트 및 게임 시작

    // 게임 시작 함수 (씬 로딩)
    void StartGame(int waveIndex)
    {
        string sceneToLoad = waveDatas[waveIndex].sceneName;
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError($"씬 이름이 설정되지 않았습니다.");
            return;
        }
        SceneManager.LoadScene(sceneToLoad); // FadeManager가 있다면 그것을 사용
    }

    // 레벨 선택 창의 웨이브 이름과 이미지 업데이트
    void UpdateLevelDisplay()
    {
        if (waveDatas == null || waveDatas.Length == 0) return;
        levelNameText.text = waveDatas[currentSelectedWaveIndex].waveName;
        waveImageDisplay.sprite = waveDatas[currentSelectedWaveIndex].waveImage;
    }

    // 모든 저장 슬롯의 요약 정보 UI 업데이트
    void UpdateAllSaveSlotsUI()
    {
        for (int i = 0; i < saveSlotUIs.Length; i++)
        {
            SaveSlot info = GameSaveManager.Instance.GetSaveSlot(i);
            saveSlotUIs[i].UpdateUI(info);
        }
    }

    // 슬롯 캐러셀의 시각적 효과 업데이트 (크기 및 텍스트)
    void UpdateCarouselVisuals()
    {
        slotIndicatorText.text = $"Slot {currentSaveSlotIndex + 1}";
        for (int i = 0; i < saveSlotUIs.Length; i++)
        {
            saveSlotUIs[i].transform.localScale = (i == currentSaveSlotIndex) ? Vector3.one * 1.2f : Vector3.one;
        }
    }
    #endregion
}
