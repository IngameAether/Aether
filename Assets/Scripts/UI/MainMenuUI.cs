using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections;

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
    public SaveSlotUI saveSlotUI;  // 단일 UI 오브젝트로 변경
    public TMP_Text slotIndicatorText;
    public Sprite arrowActiveSprite;  // 노랑색 화살표
    public Sprite arrowInactiveSprite;  // 회색 화살표
    private int currentSaveSlotIndex = 0;
    private const int MAX_SAVE_SLOTS = 3;

    [Header("레벨 데이터")]
    public WaveData[] waveDatas;
    private int currentSelectedWaveIndex = 0;

    private UIPanelState currentPanelState = UIPanelState.MainMenu;

    private Image _leftArrowImage;
    private Image _rightArrowImage;

    private void Start()
    {
        _leftArrowImage = saveSlotLeftArrow.GetComponent<Image>();
        _rightArrowImage = saveSlotRightArrow.GetComponent<Image>();

        // 게임 시작 시 메인 메뉴 패널만 보이도록 설정 (Awake 대신 Start 권장)
        ShowPanel(UIPanelState.MainMenu);

        // 버튼 이벤트 리스너 연결
        leftArrowButton?.onClick.AddListener(OnLeftArrowButtonClick);
        rightArrowButton?.onClick.AddListener(OnRightArrowButtonClick);
        playLevelButton?.onClick.AddListener(OnPlayLevelButtonClick);

        saveSlotLeftArrow?.onClick.AddListener(OnSaveSlotLeftArrowClick);
        saveSlotRightArrow?.onClick.AddListener(OnSaveSlotRightArrowClick);
        selectSlotButton?.onClick.AddListener(OnSelectSlotButtonClick);

        // 다음 게임을 위해 PopUpManager의 상태를 리셋합니다.
        PopUpManager.ResetInitialBookFlag();
    }

    private void OnDestroy()
    {
        // 메인 메뉴 UI가 파괴될 때 미니맵 캐시를 정리하여 메모리 누수 방지
        SaveSlotUI.ClearMinimapCache();
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
                UpdateCurrentSlotUI();
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
    // [이 슬롯으로 플레이] 버튼 -> 세이브 파일이 있으면 확인 후 게임 로드, 없으면 레벨 선택
    public void OnSelectSlotButtonClick()
    {
        StartCoroutine(HandleSlotSelection());
    }

    private IEnumerator HandleSlotSelection()
    {
        GameSaveManager.Instance.SelectedSlotIndex = currentSaveSlotIndex;
        SaveSlot slotInfo = GameSaveManager.Instance.GetSaveSlot(currentSaveSlotIndex);

        // 세이브 파일이 있으면 확인 팝업 표시
        if (!slotInfo.isEmpty)
        {
            bool? userChoice = null;

            PopUpManager.Instance.OpenConfirmPopUp(
                "이어서 하시겠습니까?",
                (choice) => { userChoice = choice; },
                pauseGame: false
            );

            yield return new WaitUntil(() => userChoice.HasValue);

            // 예 선택 -> 게임 로드
            if (userChoice.Value)
            {
                var loadTask = GameSaveManager.Instance.LoadGameAsync(currentSaveSlotIndex);
                yield return new WaitUntil(() => loadTask.IsCompleted);

                GameSaveDataInfo saveData = loadTask.Result;
                if (saveData != null)
                {
                    Debug.Log($"세이브 데이터 로드 성공. Wave: {saveData.currentWave}");
                    StartGame(0);
                }
                else
                {
                    Debug.LogError("세이브 데이터 로드 실패!");
                }
            }
            // 아니오 선택할시 레벨 선택 화면으로
            else
            {
                ShowPanel(UIPanelState.LevelSelect);
            }
        }
        // 세이브 파일이 없으면 레벨 선택 화면으로
        else
        {
            ShowPanel(UIPanelState.LevelSelect);
        }
    }

    void OnSaveSlotLeftArrowClick()
    {
        currentSaveSlotIndex = (currentSaveSlotIndex - 1 + MAX_SAVE_SLOTS) % MAX_SAVE_SLOTS;
        UpdateCurrentSlotUI();
        UpdateCarouselVisuals();
    }
    void OnSaveSlotRightArrowClick()
    {
        currentSaveSlotIndex = (currentSaveSlotIndex + 1) % MAX_SAVE_SLOTS;
        UpdateCurrentSlotUI();
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

    // 현재 선택된 슬롯의 정보를 UI에 업데이트
    void UpdateCurrentSlotUI()
    {
        if (saveSlotUI != null)
        {
            SaveSlot info = GameSaveManager.Instance.GetSaveSlot(currentSaveSlotIndex);
            saveSlotUI.UpdateUI(info);
        }
    }

    // 슬롯 캐러셀의 시각적 효과 업데이트 (화살표 상태 및 텍스트)
    void UpdateCarouselVisuals()
    {
        slotIndicatorText.text = $"Slot {currentSaveSlotIndex + 1}";

        // 첫 번째 슬롯(0)일 때 왼쪽 버튼 비활성화
        if (saveSlotLeftArrow != null)
        {
            bool isLeftActive = currentSaveSlotIndex > 0;
            saveSlotLeftArrow.interactable = isLeftActive;

            // 스프라이트를 활성화/비활성화 상태에 따라 변경
            _leftArrowImage.sprite = isLeftActive ? arrowActiveSprite : arrowInactiveSprite;
            _leftArrowImage.transform.localScale = (isLeftActive ? Vector3Int.one * -1 : Vector3Int.one);
        }

        // 마지막 슬롯일 때 오른쪽 버튼 비활성화
        if (saveSlotRightArrow != null)
        {
            bool isRightActive = currentSaveSlotIndex < MAX_SAVE_SLOTS - 1;
            saveSlotRightArrow.interactable = isRightActive;

            // 스프라이트를 활성화/비활성화 상태에 따라 변경
            _rightArrowImage.sprite = isRightActive ? arrowActiveSprite : arrowInactiveSprite;
            _rightArrowImage.transform.localScale = (isRightActive ? Vector3Int.one : Vector3Int.one * -1);
        }
    }
    #endregion
}
