using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using UnityEngine.Rendering.PostProcessing; 
using System.Collections;

public enum EScene
{
    MainMenu,
    InGame,
    // 필요한 다른 씬 추가
}

// UI 패널 상태를 나타내는 열거형
public enum UIPanelState
{
    None,
    MainMenu,
    LevelSelect,
    LoadSave,
    Settings
}

public class MainMenuUIManager : MonoBehaviour
{
    // 인스펙터에서 연결할 UI 패널들
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject loadSavePanel; // 새로 추가
    public GameObject settingsPanel;

    // 현재 활성화된 패널 상태 추적
    private UIPanelState currentPanelState = UIPanelState.None;
    // 설정 창 열기 전 상태 저장 (설정 닫고 돌아갈 곳)
    private UIPanelState stateBeforeSettings = UIPanelState.None;

    // 선택된 단계 인덱스 저장
    private int selectedLevelIndex = -1;

    public PostProcessVolume postProcessVolume; // 씬에 있는 Post Process Volume 연결
    private DepthOfField depthOfFieldSetting; // Depth of Field 설정 참조

    public float blurDuration = 0.5f; // 블러 전환 시간

    public float startAperture = 32f; // 블러가 약할 때 (조리개 닫힘)
    public float endAperture = 1f;   // 블러가 강할 때 (조리개 열림)


    private void Awake()
    {
        // 게임 시작 시 메인 메뉴 패널만 보이도록 설정
        ShowPanel(UIPanelState.MainMenu);
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGetSettings(out depthOfFieldSetting);
            if (depthOfFieldSetting != null)
            {
                
                // 다른 필요한 Depth of Field 속성들도 Override(true)로 활성화해야 스크립트 제어 가능
                depthOfFieldSetting.focusDistance.overrideState = true; 
                depthOfFieldSetting.focalLength.overrideState = true;   
                depthOfFieldSetting.aperture.overrideState = true;
                depthOfFieldSetting.aperture.value = startAperture;
                depthOfFieldSetting.kernelSize.overrideState = true; // 커널 사이즈도 활성화
                depthOfFieldSetting.aperture.value = startAperture;
            }
            else
            {
                Debug.LogWarning("Post Process Volume에서 Depth of Field 설정을 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("Post Process Volume이 연결되지 않았습니다.");
        }
    }

    // 특정 패널만 활성화하고 다른 패널은 비활성화하는 함수
    void ShowPanel(UIPanelState targetPanel)
    {
        // 설정 창을 열기 전 상태를 저장 (설정 창에서 돌아올 때 사용)
        if (targetPanel == UIPanelState.Settings)
        {
            stateBeforeSettings = currentPanelState; // 현재 상태를 저장
        }

        // 모든 패널 비활성화
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        loadSavePanel.SetActive(false);
        settingsPanel.SetActive(false);

        // 목표 패널 활성화
        switch (targetPanel)
        {
            case UIPanelState.MainMenu:
                mainMenuPanel.SetActive(true);
                break;
            case UIPanelState.LevelSelect:
                levelSelectPanel.SetActive(true);
                break;
            case UIPanelState.LoadSave:
                loadSavePanel.SetActive(true);
                break;
            case UIPanelState.Settings:
                settingsPanel.SetActive(true);
                break;
        }

        // 현재 상태 업데이트 (설정 창은 예외적으로 처리)
        if (targetPanel != UIPanelState.Settings)
        {
            currentPanelState = targetPanel;
        }
    }

    // --- 버튼 클릭 이벤트 핸들러 함수들 ---

    // 메인 메뉴에서 '시작' 버튼 클릭 시
    public void OnStartButtonClick()
    {
        ShowPanel(UIPanelState.LevelSelect);
        StartCoroutine(AnimateDepthOfFieldBlur(startAperture, endAperture, blurDuration));
    }

    // 메인 메뉴, 단계 선택, 로드/세이브 창에서 '설정' 버튼 클릭 시 (항상 보이는 버튼에 연결)
    public void OnSettingsButtonClick()
    {
        // ShowPanel 함수 내에서 stateBeforeSettings가 자동으로 저장됨
        ShowPanel(UIPanelState.Settings);
    }

    // 단계 선택 창에서 특정 단계 버튼 클릭 시
    public void OnLevelButtonClick(int levelIndex)
    {
        selectedLevelIndex = levelIndex; // 선택된 단계 저장
        ShowPanel(UIPanelState.LoadSave); // 로드/세이브 선택 창 표시
        // TODO: LoadSavePanel에 선택된 단계 정보를 표시하는 UI가 있다면 업데이트
    }

    // 로드/세이브 창에서 '새 게임 시작' 버튼 클릭 시
    public void OnNewGameButtonClick()
    {
        if (selectedLevelIndex != -1)
        {
            StartGame(selectedLevelIndex, false); // 새 게임 시작 (loadSave = false)
        }
        else
        {
            Debug.LogError("단계가 선택되지 않았습니다!"); // 오류 처리
            ShowPanel(UIPanelState.MainMenu); // 또는 다른 적절한 처리
        }
    }

    // 로드/세이브 창에서 '세이브 파일 불러오기' 버튼 클릭 시
    public void OnLoadGameButtonClick()
    {
        if (selectedLevelIndex != -1)
        {
            // TODO: 실제 세이브 파일 존재 여부 확인 로직 추가
            bool saveFileExists = CheckSaveFile(selectedLevelIndex); // 예시 함수

            if (saveFileExists)
            {
                StartGame(selectedLevelIndex, true); // 세이브 불러오기 (loadSave = true)
            }
            else
            {
                Debug.LogWarning($"Level {selectedLevelIndex}에 대한 세이브 파일이 없습니다. 새 게임을 시작합니다.");
                StartGame(selectedLevelIndex, false); // 세이브 파일 없으면 새 게임 시작
            }
        }
        else
        {
            Debug.LogError("단계가 선택되지 않았습니다!"); // 오류 처리
            ShowPanel(UIPanelState.MainMenu); // 또는 다른 적절한 처리
        }
    }

    // 실제 게임 시작 함수 (GameManager 호출 또는 씬 로드)
    void StartGame(int levelIndex, bool loadSave)
    {
        Debug.Log($"게임 시작: Level {levelIndex}, 세이브 불러오기: {loadSave}");
    }

    // 설정 적용 버튼 클릭 시
    public void OnApplySettingsButtonClick()
    {
        // TODO: 설정 UI 요소들(슬라이더, 토글 등)의 값을 읽어서 적용 및 저장
        Debug.Log("설정 적용됨");
        // 설정 적용 후 이전 상태로 돌아가기
        ShowPanel(stateBeforeSettings);
    }

    public void OnCancelSettingsButtonClick()
    {
        Debug.Log("설정 취소됨");
        ShowPanel(stateBeforeSettings);
    }

    // '뒤로가기' 버튼 클릭 시 (각 패널에 있는 뒤로가기 버튼에 연결)
    public void OnBackButtonClick()
    {
        switch (currentPanelState)
        {
            case UIPanelState.LevelSelect:
                ShowPanel(UIPanelState.MainMenu); // 단계 선택 -> 메인 메뉴
                StartCoroutine(AnimateDepthOfFieldBlur(endAperture, startAperture, blurDuration));
                break;
            case UIPanelState.LoadSave:
                ShowPanel(UIPanelState.LevelSelect); // 로드/세이브 -> 단계 선택
                break;
            case UIPanelState.Settings:
                ShowPanel(stateBeforeSettings); // 설정 -> 설정 열기 전 상태
                break;
            default:
                // 예외 처리 또는 아무것도 안 함
                break;
        }
    }

    // 게임 종료 버튼 클릭 시
    public void OnQuitGameButtonClick()
    {
        Debug.Log("게임 종료");
        Application.Quit(); // 실제 빌드에서만 작동
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 테스트 시 사용
#endif
    }

    IEnumerator AnimateDepthOfFieldBlur(float startValue, float endValue, float duration, bool isFocalLength = false)
    {
        if (depthOfFieldSetting == null) yield break; // Depth of Field 설정이 없으면 코루틴 종료

        float startTime = Time.time;
        float endTime = startTime + duration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration; // 0에서 1까지 진행률
            float currentValue = Mathf.Lerp(startValue, endValue, t);

            if (isFocalLength)
            {
                // Focal Length 조절
                depthOfFieldSetting.focalLength.Override(currentValue);
            }
            else
            {
                // Aperture 조절
                depthOfFieldSetting.aperture.Override(currentValue);
            }

            yield return null; // 다음 프레임까지 대기
        }

        // 애니메이션 완료 후 최종 값 설정
        if (isFocalLength)
        {
            depthOfFieldSetting.focalLength.Override(endValue);
        }
        else
        {
            depthOfFieldSetting.aperture.Override(endValue);
        }
    }

    // TODO: 실제 세이브 파일 존재 여부 확인 로직 구현
    bool CheckSaveFile(int levelIndex)
    {
        return false;
    }
}