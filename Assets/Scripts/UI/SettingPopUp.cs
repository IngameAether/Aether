using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingPopUp : MonoBehaviour
{
    [Header("UI 참조")]
    public Slider bgmSlider;
    public TMP_Text bgmValueText;
    public Toggle bgmMuteToggle;
    public Slider sfxSlider;
    public TMP_Text sfxValueText;
    public Toggle sfxMuteToggle;

    private void Awake()
    {
        // 각 UI 요소에 이벤트 리스너(기능) 연결
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        bgmMuteToggle.onValueChanged.AddListener(OnBGMMuteToggled);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteToggled);
    }

    private void OnEnable()
    {
        // 팝업이 활성화될 때마다 현재 사운드 설정을 UI에 반영
        UpdateSoundSettingsUIFromPrefs();
    }

    // --- 사운드 설정 관련 메서드 ---
    private void UpdateSoundSettingsUIFromPrefs()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 5f);
        bgmMuteToggle.isOn = PlayerPrefs.GetInt("BGMMute", 0) == 1;
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 5f);
        sfxMuteToggle.isOn = PlayerPrefs.GetInt("SFXMute", 0) == 1;

        bgmValueText.text = bgmSlider.value.ToString("F0");
        sfxValueText.text = sfxSlider.value.ToString("F0");
    }

    private void OnBGMVolumeChanged(float value)
    {
        AudioManager.Instance.SetGroupVolume("BGMVolume", value);
        bgmValueText.text = value.ToString("F0");
        if (bgmMuteToggle.isOn && value > 0) bgmMuteToggle.isOn = false;

        // BGM 볼륨 변경 즉시 모든 설정을 저장 
        SaveCurrentSettings();
    }

    private void OnBGMMuteToggled(bool isMuted)
    {
        AudioManager.Instance.SetGroupMute("BGMMute", isMuted);

        // BGM 음소거 변경 즉시 모든 설정을 저장 
        SaveCurrentSettings();
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetGroupVolume("SFXVolume", value);
        sfxValueText.text = value.ToString("F0");
        if (sfxMuteToggle.isOn && value > 0) sfxMuteToggle.isOn = false;

        // SFX 볼륨 변경 즉시 모든 설정을 저장
        SaveCurrentSettings();
    }

    private void OnSFXMuteToggled(bool isMuted)
    {
        AudioManager.Instance.SetGroupMute("SFXMute", isMuted);

        // SFX 음소거 변경 즉시 모든 설정을 저장
        SaveCurrentSettings();
    }

    /// <summary>
    /// 현재 UI 상태를 PlayerPrefs에 저장하는 공용 함수
    /// </summary>
    private void SaveCurrentSettings()
    {
        AudioManager.Instance.SaveSoundSettings(bgmSlider.value, bgmMuteToggle.isOn, sfxSlider.value, sfxMuteToggle.isOn);
        Debug.Log("사운드 설정이 자동으로 저장되었습니다.");
    }
}
