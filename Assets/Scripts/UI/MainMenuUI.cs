using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using UnityEngine.Rendering.PostProcessing; 
using System.Collections;
using TMPro;

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
    None,
    MainMenu,
    LevelSelect,
    LoadSave,
    Settings
}

public class MainMenuUI : MonoBehaviour
{
    // 인스펙터에서 연결할 UI 패널들
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject loadSavePanel; // 새로 추가
    public GameObject settingsPanel;

    // 블러처리된 배경 이미지
    public GameObject blurBackgroundImage;

    public TMP_Text levelNameText; // 레벨 이름을 표시할 UI Text
    public Image waveImageDisplay; // Wave 이미지 표시할 UI Image
    public Button leftArrowButton; // 왼쪽 화살표 버튼
    public Button rightArrowButton; // 오른쪽 화살표 버튼
    public Button playLevelButton; // 선택된 레벨을 플레이할 버튼

    public WaveData[] waveDatas; // Wave 데이터를 담을 배열
    private int currentSelectedWaveIndex = 0; // 현재 선택된 레벨의 인덱스

    // 현재 활성화된 패널 상태 추적
    private UIPanelState currentPanelState = UIPanelState.None;
    // 설정 창 열기 전 상태 저장 (설정 닫고 돌아갈 곳)
    private UIPanelState stateBeforeSettings = UIPanelState.None;

    [Header("설정창")]
    public Slider bgmSlider;
    public TMP_Text bgmValueText;
    public Toggle bgmMuteToggle;
    public Slider sfxSlider;
    public TMP_Text sfxValueText;
    public Toggle sfxMuteToggle;
    public UnityEngine.Audio.AudioMixer masterMixer;


    private void Awake()
    {
        // 게임 시작 시 메인 메뉴 패널만 보이도록 설정
        ShowPanel(UIPanelState.MainMenu);
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(OnLeftArrowButtonClick);
        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(OnRightArrowButtonClick);
        if (playLevelButton != null)
            playLevelButton.onClick.AddListener(OnPlayLevelButtonClick); // 새 버튼 이벤트 연결

        // 사운드 설정 UI 
        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        if (bgmMuteToggle != null)
            bgmMuteToggle.onValueChanged.AddListener(OnBGMMuteToggled);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (sfxMuteToggle != null)
            sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteToggled);
        LoadSavedSoundSettings();
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

        // 블러 배경 이미지 활성화/비활성화
        bool needsBlurBackground = (targetPanel == UIPanelState.Settings || targetPanel == UIPanelState.LoadSave || targetPanel == UIPanelState.LevelSelect);
        if (blurBackgroundImage != null)
        {
            blurBackgroundImage.SetActive(needsBlurBackground);
        }

        // 목표 패널 활성화
        switch (targetPanel)
        {
            case UIPanelState.MainMenu:
                mainMenuPanel.SetActive(true);
                break;
            case UIPanelState.LevelSelect:
                levelSelectPanel.SetActive(true);
                UpdateLevelDisplay();
                break;
            case UIPanelState.LoadSave:
                loadSavePanel.SetActive(true);
                break;
            case UIPanelState.Settings:
                settingsPanel.SetActive(true);
                break;
        }
        // 현재 상태 업데이트 (설정 창은 예외적으로 처리)
        currentPanelState = targetPanel;
    }

    // --- 버튼 클릭 이벤트 핸들러 함수들 ---

    // 메인 메뉴에서 '시작' 버튼 클릭 시
    public void OnStartButtonClick()
    {
        ShowPanel(UIPanelState.LevelSelect);
    }

    // 메인 메뉴, 단계 선택, 로드/세이브 창에서 '설정' 버튼 클릭 시 (항상 보이는 버튼에 연결)
    public void OnSettingsButtonClick()
    {
        // ShowPanel 함수 내에서 stateBeforeSettings가 자동으로 저장됨
        ShowPanel(UIPanelState.Settings);
    }

    // 왼쪽 화살표 버튼 클릭 시 (레벨 선택)
    public void OnLeftArrowButtonClick()
    {
        currentSelectedWaveIndex--;
        if (currentSelectedWaveIndex < 0)
        {
            currentSelectedWaveIndex = waveDatas.Length - 1; // 마지막 레벨로 순환
        }
        UpdateLevelDisplay();
    }
    // 오른쪽 화살표 버튼 클릭 시 (레벨 선택)
    public void OnRightArrowButtonClick()
    {
        currentSelectedWaveIndex++;
        if (currentSelectedWaveIndex >= waveDatas.Length)
        {
            currentSelectedWaveIndex = 0; // 첫 번째 레벨로 순환
        }
        UpdateLevelDisplay();
    }
    // 선택된 레벨을 플레이/확정 버튼 클릭 시
    public void OnPlayLevelButtonClick()
    {
        if (currentSelectedWaveIndex != -1) // -1은 초기값으로, 여기서는 0부터 시작하므로 항상 유효
        {
            ShowPanel(UIPanelState.LoadSave); // 로드/세이브 선택 창 표시
        }
        else
        {
            Debug.LogError("레벨이 선택되지 않았습니다!");
            ShowPanel(UIPanelState.MainMenu);
        }
    }

    // 로드/세이브 창에서 '새 게임 시작' 버튼 클릭 시
    public void OnNewGameButtonClick()
    {
        StartGame(currentSelectedWaveIndex, false);
    }

    // 로드/세이브 창에서 '세이브 파일 불러오기' 버튼 클릭 시
    public void OnLoadGameButtonClick()
    {
        if (waveDatas.Length > 0) // waveDatas가 비어있지 않은지 확인
        {
            bool saveFileExists = CheckSaveFile(currentSelectedWaveIndex);

            if (saveFileExists)
            {
                StartGame(currentSelectedWaveIndex, true); // 세이브 불러오기 (loadSave = true)
            }
            else
            {
                Debug.LogWarning($"Wave {currentSelectedWaveIndex}에 대한 세이브 파일이 없습니다. 새 게임을 시작합니다.");
                StartGame(currentSelectedWaveIndex, false); // 세이브 파일 없으면 새 게임 시작
            }
        }
        else
        {
            Debug.LogError("Wave 데이터가 설정되지 않았습니다!"); // 오류 처리
            ShowPanel(UIPanelState.MainMenu); // 또는 다른 적절한 처리
        }
    }

    // 실제 게임 시작 함수 (GameManager 호출 또는 씬 로드)
    void StartGame(int waveIndex, bool loadSave)
    {
        // 해당 WaveData에서 씬 이름을 가져옵니다.
        string sceneToLoad = waveDatas[waveIndex].sceneName;

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
            Debug.Log($"씬 로드 시작: {sceneToLoad}");
        }
        else
        {
            Debug.LogError($"Wave {waveIndex + 1}에 대한 씬 이름이 설정되지 않았습니다!");
        }
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
                break;
            case UIPanelState.LoadSave:
                ShowPanel(UIPanelState.LevelSelect); // 로드/세이브 -> 단계 선택
                break;
            case UIPanelState.Settings:
                ShowPanel(stateBeforeSettings); // 설정 -> 설정 열기 전 상태
                break;
            default:
                Debug.LogWarning("현재 패널에서 '뒤로가기' 동작이 정의되지 않았습니다: " + currentPanelState);
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
    // 레벨 정보를 UI에 업데이트하는 함수
    void UpdateLevelDisplay()
    {
        if (waveDatas != null && waveDatas.Length > 0)
        {
            // 텍스트 업데이트
            if (levelNameText != null)
            {
                levelNameText.text = waveDatas[currentSelectedWaveIndex].waveName;
            }
            else
            {
                Debug.LogWarning("LevelNameText UI에 연결되지 않았습니다.");
            }
            // 이미지 업데이트
            if (waveImageDisplay != null)
            {
                waveImageDisplay.sprite = waveDatas[currentSelectedWaveIndex].waveImage;
            }
            else
            {
                Debug.LogWarning("WaveImageDisplay UI에 연결되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogWarning("WaveDatas 배열이 비어 있습니다. Wave 데이터를 추가해주세요.");
            // 데이터가 없을 경우 UI를 비활성화하거나 기본값 설정
            if (levelNameText != null) levelNameText.text = "No Waves";
            if (waveImageDisplay != null) waveImageDisplay.sprite = null;
        }
    }

    // TODO: 실제 세이브 파일 존재 여부 확인 로직 구현
    bool CheckSaveFile(int waveIndex)
    {
        return false;
    }

    // 사운드 설정 관련 메서드
    // 저장된 사운드 설정을 로드하여 게임에 적용하고 UI에 반영
    private void LoadSavedSoundSettings()
    {
        // PlayerPrefs에서 값을 로드하거나 기본값 설정
        // 배경음악
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 5f); // 기본값 5
        bool bgmMute = PlayerPrefs.GetInt("BGMMute", 0) == 1; // 기본값 음소거 아님 (0)
        // 효과음
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 5f); // 기본값 5
        bool sfxMute = PlayerPrefs.GetInt("SFXMute", 0) == 1; // 기본값 음소거 아님 (0)

        // 실제 Audio Mixer에 적용
        SetMixerVolume("BGMVolume", bgmVolume);
        SetMixerMute("BGMMute", bgmMute);

        SetMixerVolume("SFXVolume", sfxVolume);
        SetMixerMute("SFXMute", sfxMute);

        // UI에 반영 (설정 패널이 열려있지 않아도 초기값은 반영)
        if (bgmSlider != null)
            bgmSlider.value = bgmVolume;
        if (bgmMuteToggle != null)
            bgmMuteToggle.isOn = bgmMute;
        if (sfxSlider != null)
            sfxSlider.value = sfxVolume;
        if (sfxMuteToggle != null)
            sfxMuteToggle.isOn = sfxMute;

        // 슬라이더 값 텍스트 업데이트
        if (bgmValueText != null)
            bgmValueText.text = bgmVolume.ToString("F0");
        if (sfxValueText != null)
            sfxValueText.text = sfxVolume.ToString("F0");
    }

    // UI 슬라이더/토글 값 변경 시 호출 (실시간 반영)
    private void OnBGMVolumeChanged(float value)
    {
        SetMixerVolume("BGMVolume", value);
        if (bgmValueText != null) bgmValueText.text = value.ToString("F0");
        // 슬라이더 조절 시 음소거 해제
        if (bgmMuteToggle != null && bgmMuteToggle.isOn) bgmMuteToggle.isOn = false;
    }

    private void OnBGMMuteToggled(bool isMuted)
    {
        SetMixerMute("BGMMute", isMuted);
        // 음소거 시 슬라이더 값을 0으로 (UI만)
        if (bgmSlider != null && isMuted)
            bgmSlider.value = 0;
        else if (bgmSlider != null && !isMuted)
            bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 7f); // 음소거 해제 시 저장된 값으로 복구
    }

    private void OnSFXVolumeChanged(float value)
    {
        SetMixerVolume("SFXVolume", value);
        if (sfxValueText != null)
            sfxValueText.text = value.ToString("F0");
        // 슬라이더 조절 시 음소거 해제
        if (sfxMuteToggle != null && sfxMuteToggle.isOn)
            sfxMuteToggle.isOn = false;
    }

    private void OnSFXMuteToggled(bool isMuted)
    {
        SetMixerMute("SFXMute", isMuted);
        // 음소거 시 슬라이더 값을 0으로 (UI만)
        if (sfxSlider != null && isMuted)
            sfxSlider.value = 0;
        else if (sfxSlider != null && !isMuted)
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 7f); // 음소거 해제 시 저장된 값으로 복구
    }

    // 설정 적용 버튼 클릭 시 호출 (PlayerPrefs에 저장)
    private void SaveSoundSettings()
    {
        if (bgmSlider != null) PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
        if (bgmMuteToggle != null) PlayerPrefs.SetInt("BGMMute", bgmMuteToggle.isOn ? 1 : 0);
        if (sfxSlider != null) PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        if (sfxMuteToggle != null) PlayerPrefs.SetInt("SFXMute", sfxMuteToggle.isOn ? 1 : 0);

        PlayerPrefs.Save(); // 변경사항 즉시 저장
        Debug.Log("사운드 설정 저장 완료.");
    }

    // 설정 패널 열 때 UI에 현재 설정 값을 반영 (PlayerPrefs에서 직접 로드)
    private void UpdateSoundSettingsUI()
    {
        // UI 요소들이 null이 아닌지 다시 확인하고, 현재 PlayerPrefs 값을 UI에 반영
        if (bgmSlider != null) bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 5f);
        if (bgmMuteToggle != null) bgmMuteToggle.isOn = PlayerPrefs.GetInt("BGMMute", 0) == 1;
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 5f);
        if (sfxMuteToggle != null) sfxMuteToggle.isOn = PlayerPrefs.GetInt("SFXMute", 0) == 1;

        // 슬라이더 값 텍스트 업데이트
        if (bgmValueText != null) bgmValueText.text = bgmSlider.value.ToString("F0");
        if (sfxValueText != null) sfxValueText.text = sfxSlider.value.ToString("F0");
    }

    // Audio Mixer 볼륨 설정 (0~10 스케일을 dB 스케일로 변환)
    private void SetMixerVolume(string parameterName, float volume)
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("Audio Mixer가 연결되지 않았습니다. 사운드 볼륨을 조절할 수 없습니다.");
            return;
        }

        // 0~10 스케일을 로그 스케일 dB로 변환 (0은 -80dB, 10은 0dB)
        // 볼륨이 0일 때는 -80dB (완전 소거)
        if (volume == 0)
        {
            masterMixer.SetFloat(parameterName, -80f);
        }
        else
        {
            // Mathf.Log10(volume / 10f) * 20f;
            // 0~10 -> 0.001~1.0 (선형) -> 로그 스케일 dB
            masterMixer.SetFloat(parameterName, Mathf.Log10(volume / 10f) * 20f);
        }
    }

    // Audio Mixer 그룹 음소거 설정
    private void SetMixerMute(string parameterName, bool isMuted)
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("Audio Mixer가 연결되지 않았습니다. 사운드 음소거를 조절할 수 없습니다.");
            return;
        }
        if (isMuted)
        {
            // 음소거 시 -80dB로 설정 (완전 소거)
            masterMixer.SetFloat(parameterName.Replace("Mute", "Volume"), -80f);
        }
        else
        {
            // 음소거 해제 시 저장된 볼륨값으로 복구
            float savedVolume = PlayerPrefs.GetFloat(parameterName.Replace("Mute", "Volume"), 5f);
            masterMixer.SetFloat(parameterName.Replace("Mute", "Volume"), Mathf.Log10(savedVolume / 10f) * 20f);
        }
    }
}