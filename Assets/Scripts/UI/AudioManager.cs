using UnityEngine;
using UnityEngine.Audio; // AudioMixer를 사용하기 위해 필요
using System.Collections.Generic; // Dictionary를 사용하기 위해 필요

public class AudioManager : MonoBehaviour
{
    // 싱글턴 인스턴스 (어디서든 AudioManager.Instance로 접근 가능)
    public static AudioManager Instance { get; private set; }

    [Header("오디오 믹서")]
    public AudioMixer masterMixer; // Unity 에디터에서 Main Audio Mixer 에셋을 연결

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmAudioSource; // BGM 재생용 AudioSource (인스펙터에서 연결)
    [SerializeField] private AudioSource sfxAudioSource; // SFX 재생용 AudioSource (인스펙터에서 연결)

    [Header("Audio Mixer Snapshots")]
    public AudioMixerSnapshot normalSnapshot; // 기본 상태 스냅샷 (인스펙터에서 연결)
    public AudioMixerSnapshot pausedSnapshot; // 일시 정지 상태 스냅샷 (인스펙터에서 연결)
    public AudioMixerSnapshot mutedSnapshot;  // 음소거 상태 스냅샷 (인스펙터에서 연결)

    [Header("오디오 클립")]
    // BGM 클립을 위한 Dictionary 또는 List (추후 확장 가능성을 고려)
    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;

    private Dictionary<string, AudioClip> bgmClipDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClipDictionary = new Dictionary<string, AudioClip>();

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
        if (bgmAudioSource == null) Debug.LogError("BGM AudioSource가 연결되지 않았습니다.");
        if (sfxAudioSource == null) Debug.LogError("SFX AudioSource가 연결되지 않았습니다.");

        // BGM 클립
        foreach (AudioClip clip in bgmClips)
        {
            if (clip != null && !bgmClipDictionary.ContainsKey(clip.name))
            {
                bgmClipDictionary.Add(clip.name, clip);
            }
        }
        // SFX 클립
        foreach (AudioClip clip in sfxClips)
        {
            if (clip != null && !sfxClipDictionary.ContainsKey(clip.name))
            {
                sfxClipDictionary.Add(clip.name, clip);
            }
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

    // BGM 시작
    public void PlayBGM(string bgmName, bool loop = true)
    {
        if(bgmAudioSource == null)
        {
            Debug.LogWarning("BGM AudioSource가 할당되지 않았습니다.");
            return;
        }

        if (!bgmClipDictionary.ContainsKey(bgmName))
        {
            Debug.LogWarning($"BGM 클립 '{bgmName}'을 찾을 수 없습니다.");
            return;
        }

        bgmAudioSource.clip = bgmClipDictionary[bgmName];
        bgmAudioSource.loop = loop; // 루프 여부 설정
        bgmAudioSource.Play();
        Debug.Log($"BGM '{bgmName}' 재생 시작. (Loop: {loop})");
    }

    // BGM 정지
    public void StopBGM()
    {
        if(bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
            Debug.Log("BGM 정지");
        }
    }
    // 전체 음소거 (사용자 설정에서)
    public void OnGameMuted()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.TransitionToSnapshot(AudioManager.Instance.mutedSnapshot, 0.1f);
        }
    }

    // BGM 일시 정지
    public void PauseBGM()
    {
        if(bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
            Debug.Log("BGM 일시 정지");
        }
    }
    // 게임 일시 정지 시
    public void OnGamePaused()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.TransitionToSnapshot(AudioManager.Instance.pausedSnapshot, 0.5f); // 0.5초에 걸쳐 전환
        }
    }

    // BGM 재생
    public void UnPauseBGM()
    {
        // time != 0은 시작점이 아님을 의미
        if (bgmAudioSource != null && bgmAudioSource.time != 0 && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.UnPause();
            Debug.Log("BGM 다시 재생");
        }
        else if(bgmAudioSource != null && !bgmAudioSource.isPlaying && bgmAudioSource.clip != null)
        {
            // clip이 있고 재생 중이 아니라면 새로 재생
            bgmAudioSource.Play();
            Debug.Log("BGM 재시작 (새로 재생)");
        }
    }
    // 게임 재개 시
    public void OnGameResumed()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.TransitionToSnapshot(AudioManager.Instance.normalSnapshot, 0.5f);
        }
    }

    public void PlaySFX(string sfxName)
    {
        if(sfxAudioSource == null)
        {
            Debug.LogWarning("SFX AudioSource가 할당되지 않습니다.");
            return;
        }

        if (!sfxClipDictionary.ContainsKey(sfxName))
        {
            Debug.LogWarning($"SFX 클립 '{sfxName}'을 찾을 수 없습니다.");
            return;
        }
        // PlayOneShot은 현재 AudioSource의 설정을 유지한 채 AudioClip을 한 번 재생함
        // 여러 SFX가 동시에 발생해도 서로 덮어쓰지 않고 겹쳐서 재생
        sfxAudioSource.PlayOneShot(sfxClipDictionary[sfxName]);
        Debug.Log($"SFX '{sfxName}' 재생");
    }

    public void TransitionToSnapshot(AudioMixerSnapshot snapshot, float timeToReach)
    {
        if (snapshot == null)
        {
            Debug.LogWarning("전환하려는 AudioMixer Snapshot이 null입니다.");
            return;
        }
        snapshot.TransitionTo(timeToReach);
        Debug.Log($"Audio Mixer를 스냅샷 '{snapshot.name}'으로 전환 (시간: {timeToReach}s).");
    }
}
