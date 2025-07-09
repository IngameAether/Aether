using UnityEngine;
using UnityEngine.Audio; // AudioMixer를 사용하기 위해 필요
using System.Collections.Generic; // Dictionary를 사용하기 위해 필요

public class AudioManager : MonoBehaviour
{
    // 싱글턴 인스턴스 (어디서든 AudioManager.Instance로 접근 가능)
    public static AudioManager Instance { get; private set; }

    [Header("오디오 믹서")]
    public AudioMixer masterMixer; // Unity 에디터에서 Main Audio Mixer 에셋을 연결

    private void Awake()
    {
        // 싱글턴 패턴 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 이미 인스턴스가 있으면 자신을 파괴
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
        }
        // 게임 시작 시 저장된 사운드 설정을 로드하여 적용
        LoadSoundSettingsToMixer();
    }

    // --- 사운드 제어 메서드 (MainMenuUI에서 호출) ---

    // 특정 그룹 볼륨 설정 (0~10 스케일을 dB 스케일로 변환)
    public void SetGroupVolume(string parameterName, float volume)
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("Audio Mixer가 연결되지 않았습니다. 사운드 볼륨을 조절할 수 없습니다.");
            return;
        }

        if (volume == 0)
        {
            masterMixer.SetFloat(parameterName, -80f); // 볼륨이 0일 때는 -80dB (완전 소거)
        }
        else
        {
            masterMixer.SetFloat(parameterName, Mathf.Log10(volume / 10f) * 20f); // 0~10 -> 로그 스케일 dB
        }
    }

    // 특정 그룹 음소거 설정 (볼륨 파라미터로 음소거 구현)
    public void SetGroupMute(string parameterName, bool isMuted)
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("Audio Mixer가 연결되지 않았습니다. 사운드 음소거를 조절할 수 없습니다.");
            return;
        }

        // parameterName은 "BGMMute" 또는 "SFXMute" 형태
        string volumeParameterName = parameterName.Replace("Mute", "Volume"); // "BGMVolume" 또는 "SFXVolume"

        if (isMuted)
        {
            masterMixer.SetFloat(volumeParameterName, -80f); // 음소거 시 -80dB로 설정
        }
        else
        {
            // 음소거 해제 시 PlayerPrefs에 저장된 볼륨 값으로 복구
            float savedVolume = PlayerPrefs.GetFloat(volumeParameterName, 5f); // 기본값 5
            SetGroupVolume(volumeParameterName, savedVolume); // SetGroupVolume을 재활용하여 dB 변환 적용
        }
    }

    // PlayerPrefs에서 사운드 설정을 로드하여 Audio Mixer에 적용
    public void LoadSoundSettingsToMixer()
    {
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 5f);
        bool bgmMute = PlayerPrefs.GetInt("BGMMute", 0) == 1;

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 5f);
        bool sfxMute = PlayerPrefs.GetInt("SFXMute", 0) == 1;

        SetGroupVolume("BGMVolume", bgmVolume);
        SetGroupMute("BGMMute", bgmMute);

        SetGroupVolume("SFXVolume", sfxVolume);
        SetGroupMute("SFXMute", sfxMute);

        Debug.Log("사운드 설정 로드 및 믹서 적용 완료.");
    }

    // 현재 UI 값들을 PlayerPrefs에 저장
    public void SaveSoundSettings(float bgmVal, bool bgmMuteVal, float sfxVal, bool sfxMuteVal)
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVal);
        PlayerPrefs.SetInt("BGMMute", bgmMuteVal ? 1 : 0);
        PlayerPrefs.SetFloat("SFXVolume", sfxVal);
        PlayerPrefs.SetInt("SFXMute", sfxMuteVal ? 1 : 0);

        PlayerPrefs.Save(); // 변경사항 즉시 저장
        Debug.Log("사운드 설정 PlayerPrefs에 저장 완료.");
    }
}