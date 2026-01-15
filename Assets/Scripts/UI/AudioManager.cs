using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// 추가할 소리는 여기에
public enum SfxType
{
    None,

    // UI 사운드
    Magicbook_get,
    PopUp_close,
    PopUp_open,

    // 플레이 사운드
    Screen_touch,
    Tower_combination,
    Tower_reinforce,
    Lose_life,
    Tower_Sell,

    // 게임 시작과 끝
    Game_start,
    Game_over,

    // 아쿠아 공격 사운드
    L1A,
    L2A_Flying,
    L2A_Attack,
    L3A_Attack,
    L4A_Attack,
    L4A_Prepare,
    // 대지 공격 사운드
    L1E_Flying,
    L2E_Prepare,
    L2E_Attack,
    L3E_Attack,
    L3E_Prepare,
    L4E_Attack,
    // 불 공격 사운드
    L1F,
    L2F_Flying,
    L2F_Attack,
    L3F_Attack,
    L4F_Attack,
    L4F_Prepare,
    // 공기 공격 사운드
    L1W,
    L2W_Flying,
    L2W_Attack,
    L3W_Attack,
    L4W_Attack,
    // 3단계 공격 사운드
    L3GLA_Attack,
    L3LIG_Prepare,
    L3LIG_Attack,
    L3MET_Prepare,
    L3MET_Attack,
    L3SOL_Prepare,
    L3STO_Flying,
    L3VIT_Prepare,
    L3VIT_Attack,
}
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            // _instance가 비어있는 경우 (씬에 AudioManager가 없을 때)
            if (_instance == null)
            {
                // 혹시 씬에 이미 있는지 찾아본다.
                _instance = FindObjectOfType<AudioManager>();

                // 그래도 없으면 Resources 폴더에서 프리팹을 불러와 생성한다.
                if (_instance == null)
                {
                    // "Resources" 폴더에 있는 "AudioManager" 이름의 프리팹을 찾습니다.
                    GameObject prefab = Resources.Load<GameObject>("AudioManager");
                    if (prefab != null)
                    {
                        GameObject go = Instantiate(prefab);
                        _instance = go.GetComponent<AudioManager>();
                    }
                    else
                    {
                        Debug.LogError("Resources 폴더에 'AudioManager' 프리팹이 없습니다!");
                    }
                }
            }
            return _instance;
        }
    }

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

    [System.Serializable]
    public class SfxEntry
    {
        public SfxType type;
        public AudioClip clip;
    }

    [SerializeField] private List<SfxEntry> sfxList; 

    private Dictionary<string, AudioClip> bgmClipDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<SfxType, AudioClip> sfxClipDictionary = new Dictionary<SfxType, AudioClip>();

    private void Awake()
    {
        // Awake() 함수의 싱글톤 처리 로직 변경
        // 이미 인스턴스가 존재하는데, 그게 내가 아니라면 스스로를 파괴합니다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 내가 최초의 인스턴스라면, DontDestroyOnLoad를 통해 씬 전환 시에도 파괴되지 않도록 합니다.
        // Instance 속성이 호출될 때 _instance에 자신을 할당하므로, 여기서 중복 할당할 필요는 없습니다.
        DontDestroyOnLoad(gameObject);

        // BGM 처리
        bgmClipDictionary = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in bgmClips)
        {
            if (clip != null && !bgmClipDictionary.ContainsKey(clip.name))
                bgmClipDictionary.Add(clip.name, clip);
        }
        // SFX 처리
        sfxClipDictionary = new Dictionary<SfxType, AudioClip>();
        foreach (SfxEntry entry in sfxList)
        {
            if (!sfxClipDictionary.ContainsKey(entry.type))
                sfxClipDictionary.Add(entry.type, entry.clip);
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
        // 음소거 상태 저장
        PlayerPrefs.SetInt(parameterName.Replace("Volume", "Mute"), isMuted ? 1 : 0);
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
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 5f);
        SetGroupVolume("BGMVolume", bgmVolume);

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 5f);
        SetGroupVolume("SFXVolume", sfxVolume);
    }

    // 현재 UI 값들을 PlayerPrefs에 저장하는 기능
    public void SaveSoundSettings(float bgmVal, bool bgmMuteVal, float sfxVal, bool sfxMuteVal)
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVal);
        PlayerPrefs.SetFloat("SFXVolume", sfxVal);
        PlayerPrefs.Save();
    }

    #region Playback Methods

    // AudioClip을 직접 받아서 재생하는 오버로딩 함수
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmAudioSource == null || clip == null) return;

        // 이미 똑같은 음악이 재생 중이고, 반복 설정도 같다면 끊지 않고 유지
        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying && bgmAudioSource.loop == loop)
            return;

        bgmAudioSource.clip = clip;
        bgmAudioSource.loop = loop; // 반복 여부 설정 (true: 계속 재생, false: 1번만 재생)
        bgmAudioSource.Play();
    }

    public void PlayBGM(string bgmName, bool loop = true)
    {
        if (bgmAudioSource == null) return;
        if (!bgmClipDictionary.ContainsKey(bgmName)) return;
        PlayBGM(bgmClipDictionary[bgmName], loop);
    }

    public void PlaySFX(SfxType sfxType)
    {
        if (sfxAudioSource == null || !sfxClipDictionary.ContainsKey(sfxType))
        {
            Debug.LogWarning(sfxType + " 타입에 해당하는 SFX 클립이 없습니다.");
            return;
        }
        sfxAudioSource.PlayOneShot(sfxClipDictionary[sfxType]);
    }

    public void StopBGM(float fadeDuration = 1.0f)
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            StartCoroutine(FadeOutBGM(fadeDuration));
        }
    }
    private IEnumerator FadeOutBGM(float duration)
    {
        float startVolume = bgmAudioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            // 시간이 지남에 따라 볼륨을 0으로 서서히 줄입니다.
            bgmAudioSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            timer += Time.unscaledDeltaTime; // Time.timeScale의 영향을 받지 않음
            yield return null;
        }

        bgmAudioSource.volume = 0;
        bgmAudioSource.Stop();
        bgmAudioSource.volume = startVolume; // 원래 볼륨으로 복구 (나중에 다시 재생할 때를 위해)
    }

    public void TransitionToSnapshot(AudioMixerSnapshot snapshot, float timeToReach)
    {
        if (snapshot != null)
        {
            snapshot.TransitionTo(timeToReach);
        }
    }
    #endregion

    // 포물선 공격용 (날아가는 소리, 부딧히는 소리 나눠주는 함수)
    public void PlaySFXAtPoint(SfxType sfxType, Vector3 position)
    {
        if (!sfxClipDictionary.ContainsKey(sfxType))
        {
            Debug.LogWarning(sfxType + " 타입에 해당하는 SFX 클립이 없습니다.");
            return;
        }
        AudioSource.PlayClipAtPoint(sfxClipDictionary[sfxType], position);
    }
}
