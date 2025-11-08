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
