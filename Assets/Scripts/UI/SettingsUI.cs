using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("UI 참조")]
    public Slider bgmSlider;
    public TMP_Text bgmValueText;
    public Toggle bgmMuteToggle; // BGM 음소거 토글
    public Slider sfxSlider;
    public TMP_Text sfxValueText;
    public Toggle sfxMuteToggle; // SFX 음소거 토글

    private void OnEnable()
    {
        LoadSettingsToUI();

        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        bgmMuteToggle.onValueChanged.AddListener(OnBGMMuteToggled);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteToggled);
    }

    private void OnDisable()
    {
        // 팝업이 닫힐 때 리스너를 제거하고 현재 UI 상태를 최종 저장합니다.
        bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        bgmMuteToggle.onValueChanged.RemoveListener(OnBGMMuteToggled);
        sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        sfxMuteToggle.onValueChanged.RemoveListener(OnSFXMuteToggled);

        SaveCurrentSettings(); // 여기서 최종 저장
    }

    private void LoadSettingsToUI()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 5f);
        bgmMuteToggle.isOn = PlayerPrefs.GetInt("BGMMute", 0) == 1;
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 5f);
        sfxMuteToggle.isOn = PlayerPrefs.GetInt("SFXMute", 0) == 1;

        bgmValueText.text = bgmSlider.value.ToString("F0");
        sfxValueText.text = sfxSlider.value.ToString("F0");
    }

    // --- BGM 제어 ---
    private void OnBGMVolumeChanged(float value)
    {
        AudioManager.Instance?.SetGroupVolume("BGMVolume", value);
        bgmValueText.text = value.ToString("F0");
        // 슬라이더를 움직이면 음소거는 자동으로 해제
        if (bgmMuteToggle.isOn && value > 0.1f)
        {
            bgmMuteToggle.isOn = false;
        }
    }

    private void OnBGMMuteToggled(bool isMuted)
    {
        AudioManager.Instance?.SetGroupMute("BGMMute", isMuted);
    }

    // --- SFX 제어 ---
    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetGroupVolume("SFXVolume", value);
        sfxValueText.text = value.ToString("F0");
        if (sfxMuteToggle.isOn && value > 0.1f)
        {
            sfxMuteToggle.isOn = false;
        }
    }

    private void OnSFXMuteToggled(bool isMuted)
    {
        AudioManager.Instance?.SetGroupMute("SFXMute", isMuted);
    }

    // 팝업이 닫힐 때 현재 UI 상태를 저장하는 함수
    private void SaveCurrentSettings()
    {
        AudioManager.Instance?.SaveSoundSettings(
            bgmSlider.value,
            bgmMuteToggle.isOn,
            sfxSlider.value,
            sfxMuteToggle.isOn
        );
        Debug.Log("사운드 설정이 저장되었습니다.");
    }
}
