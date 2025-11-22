using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingsUI : MonoBehaviour
{
    [Header("UI 참조")]
    public Slider bgmSlider;
    public TMP_Text bgmValueText;
    public Toggle bgmMuteToggle; // BGM 음소거 토글
    public Slider sfxSlider;
    public TMP_Text sfxValueText;
    public Toggle sfxMuteToggle; // SFX 음소거 토글

    [Header("Scene Navigation")]
    [SerializeField] private GameObject goToMainButton; // 메인으로 가기 버튼 오브젝트
    [SerializeField] private Button mainButtonComponent; // 버튼의 클릭 이벤트를 위해

    private void OnEnable()
    {
        LoadSettingsToUI();

        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        bgmMuteToggle.onValueChanged.AddListener(OnBGMMuteToggled);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteToggled);

        // 메인 메뉴 버튼 리스너 연결
        if (mainButtonComponent != null)
        {
            mainButtonComponent.onClick.AddListener(OnClickGoToMain);
        }
        // 현재 씬이 메인 메뉴라면 버튼을 숨깁니다.
        if (SceneManager.GetActiveScene().name == "MainMenuScene")
        {
            if (goToMainButton != null) goToMainButton.SetActive(false);
        }
        else
        {
            if (goToMainButton != null) goToMainButton.SetActive(true);
        }
    }

    private void OnDisable()
    {
        // 팝업이 닫힐 때 리스너를 제거하고 현재 UI 상태를 최종 저장합니다.
        bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        bgmMuteToggle.onValueChanged.RemoveListener(OnBGMMuteToggled);
        sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        sfxMuteToggle.onValueChanged.RemoveListener(OnSFXMuteToggled);

        // 버튼 리스너 해제
        if (mainButtonComponent != null)
        {
            mainButtonComponent.onClick.RemoveListener(OnClickGoToMain);
        }

        SaveCurrentSettings(); // 여기서 최종 저장
    }

    // 메인으로 가기 버튼 클릭 시 실행될 함수
    private void OnClickGoToMain()
    {
        // 팝업을 닫는 명령을 추가합니다.
        if (PopUpManager.Instance != null)
        {
            PopUpManager.Instance.CloseCurrentPopUp();
        }

        // 팝업이 열려있어서 멈춘 시간을 다시 흐르게 합니다.
        Time.timeScale = 0.5f;

        // FadeManager를 통해 부드럽게 메인 메뉴로 이동합니다.
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.TransitionToScene("MainMenuScene");
        }
        else
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }

    private void LoadSettingsToUI()
    {
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 5f);
        bool bgmMuted = PlayerPrefs.GetInt("BGMMute", 0) == 1; // 1이면 음소거(true)
        bgmSlider.value = bgmVolume;
        bgmMuteToggle.isOn = !bgmMuted; // 체크 상태 = 음소거가 아닐 때 (true)
        bgmSlider.interactable = !bgmMuted; // 음소거가 아닐 때만 슬라이더 활성화
        bgmValueText.text = bgmSlider.value.ToString("F0");

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 5f);
        bool sfxMuted = PlayerPrefs.GetInt("SFXMute", 0) == 1;
        sfxSlider.value = sfxVolume;
        sfxMuteToggle.isOn = !sfxMuted; 
        sfxSlider.interactable = !sfxMuted; 
        sfxValueText.text = sfxSlider.value.ToString("F0");

        bgmValueText.text = bgmSlider.value.ToString("F0");
        sfxValueText.text = sfxSlider.value.ToString("F0");
    }

    // --- BGM 제어 ---
    private void OnBGMVolumeChanged(float value)
    {
        AudioManager.Instance?.SetGroupVolume("BGMVolume", value);
        bgmValueText.text = value.ToString("F0");
        // 슬라이더를 움직이면, 음소거 토글이 꺼져있을 경우(음소거 상태) 자동으로 켬
        if (!bgmMuteToggle.isOn)
        {
            bgmMuteToggle.isOn = false;
        }
    }

    private void OnBGMMuteToggled(bool isOn)
    {
        // 실제 음소거 상태(isMuted)는 isOn의 '반대'
        bool isMuted = !isOn;
        AudioManager.Instance?.SetGroupMute("BGMVolume", isMuted);

        // 토글 상태에 따라 슬라이더 활성화/비활성화
        bgmSlider.interactable = isOn;
    }

    // --- SFX 제어 ---
    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetGroupVolume("SFXVolume", value);
        sfxValueText.text = value.ToString("F0");
        if (!sfxMuteToggle.isOn)
        {
            sfxMuteToggle.isOn = true;
        }
    }

    private void OnSFXMuteToggled(bool isOn)
    {
        bool isMuted = !isOn;
        AudioManager.Instance?.SetGroupMute("SFXVolume", isMuted);
        sfxSlider.interactable = isOn;
    }

    // 팝업이 닫힐 때 현재 UI 상태를 저장하는 함수
    private void SaveCurrentSettings()
    {
        AudioManager.Instance?.SaveSoundSettings(
            bgmSlider.value,
            !bgmMuteToggle.isOn,
            sfxSlider.value,
            !sfxMuteToggle.isOn
        );
        Debug.Log("사운드 설정이 저장되었습니다.");
    }
}
