using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("오디오 믹서")]
    public AudioMixer masterMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Audio Mixer Snapshots")]
    public AudioMixerSnapshot normalSnapshot;
    public AudioMixerSnapshot mutedSnapshot;

    [Header("오디오 클립")]
    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;

    private Dictionary<string, AudioClip> bgmClipDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClipDictionary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (AudioClip clip in bgmClips)
        {
            if (clip != null && !bgmClipDictionary.ContainsKey(clip.name))
                bgmClipDictionary.Add(clip.name, clip);
        }
        foreach (AudioClip clip in sfxClips)
        {
            if (clip != null && !sfxClipDictionary.ContainsKey(clip.name))
                sfxClipDictionary.Add(clip.name, clip);
        }
    }

    private void Start()
    {
        // 게임 시작 시 저장된 사운드 설정을 로드하여 적용
        LoadSoundSettings();
    }

    // 볼륨 설정: 0~10 스케일을 dB로 변환
    public void SetGroupVolume(string parameterName, float volume)
    {
        // 볼륨이 0에 가까우면 -80dB(완전 소거), 아니면 로그 스케일로 변환
        float db = volume > 0.1f ? Mathf.Log10(volume / 10f) * 20f : -80f;
        masterMixer.SetFloat(parameterName, db);
    }

    // 개별 그룹 음소거 설정
    public void SetGroupMute(string parameterName, bool isMuted)
    {
        string volumeParameterName = parameterName.Replace("Mute", "Volume"); // "BGMVolume" 또는 "SFXVolume"

        if (isMuted)
        {
            masterMixer.SetFloat(volumeParameterName, -80f); // 음소거 시 -80dB로 설정
        }
        else
        {
            // 음소거 해제 시 PlayerPrefs에 저장된 볼륨 값으로 복구
            float savedVolume = PlayerPrefs.GetFloat(volumeParameterName, 5f);
            SetGroupVolume(volumeParameterName, savedVolume);
        }
    }

    // 전체 음소거 설정 (스냅샷 사용)
    public void SetMasterMute(bool isMuted)
    {
        if (isMuted)
        {
            TransitionToSnapshot(mutedSnapshot, 0.1f);
        }
        else
        {
            TransitionToSnapshot(normalSnapshot, 0.1f);
        }
        // 음소거 상태 저장
        PlayerPrefs.SetInt("MasterMute", isMuted ? 1 : 0);
    }

    // 사운드 설정 로드
    public void LoadSoundSettings()
    {
        // 볼륨 로드
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 5f);
        bool bgmMute = PlayerPrefs.GetInt("BGMMute", 0) == 1;
        SetGroupVolume("BGMVolume", bgmVolume);
        SetGroupMute("BGMMute", bgmMute);

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 5f);
        bool sfxMute = PlayerPrefs.GetInt("SFXMute", 0) == 1;
        SetGroupVolume("SFXVolume", sfxVolume);
        SetGroupMute("SFXMute", sfxMute);

        Debug.Log("사운드 설정 로드 및 믹서 적용 완료.");
    }

    // 현재 UI 값들을 PlayerPrefs에 저장하는 기능
    public void SaveSoundSettings(float bgmVal, bool bgmMuteVal, float sfxVal, bool sfxMuteVal)
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVal);
        PlayerPrefs.SetInt("BGMMute", bgmMuteVal ? 1 : 0);
        PlayerPrefs.SetFloat("SFXVolume", sfxVal);
        PlayerPrefs.SetInt("SFXMute", sfxMuteVal ? 1 : 0);
        PlayerPrefs.Save();
    }

    #region Playback Methods
    public void PlayBGM(string bgmName, bool loop = true)
    {
        if (bgmAudioSource == null) return;
        if (!bgmClipDictionary.ContainsKey(bgmName)) return;

        bgmAudioSource.clip = bgmClipDictionary[bgmName];
        bgmAudioSource.loop = loop;
        bgmAudioSource.Play();
    }

    public void PlaySFX(string sfxName)
    {
        if (sfxAudioSource == null) return;
        if (!sfxClipDictionary.ContainsKey(sfxName)) return;
        sfxAudioSource.PlayOneShot(sfxClipDictionary[sfxName]);
    }

    public void TransitionToSnapshot(AudioMixerSnapshot snapshot, float timeToReach)
    {
        if (snapshot != null)
        {
            snapshot.TransitionTo(timeToReach);
        }
    }
    #endregion
}
