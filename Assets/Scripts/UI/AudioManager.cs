using UnityEngine;
using UnityEngine.Audio; // AudioMixer�� ����ϱ� ���� �ʿ�
using System.Collections.Generic; // Dictionary�� ����ϱ� ���� �ʿ�

public class AudioManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ� (��𼭵� AudioManager.Instance�� ���� ����)
    public static AudioManager Instance { get; private set; }

    [Header("����� �ͼ�")]
    public AudioMixer masterMixer; // Unity �����Ϳ��� Main Audio Mixer ������ ����

    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // �̹� �ν��Ͻ��� ������ �ڽ��� �ı�
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� �ı����� �ʵ��� ����
        }
        // ���� ���� �� ����� ���� ������ �ε��Ͽ� ����
        LoadSoundSettingsToMixer();
    }

    // --- ���� ���� �޼��� (MainMenuUI���� ȣ��) ---

    // Ư�� �׷� ���� ���� (0~10 �������� dB �����Ϸ� ��ȯ)
    public void SetGroupVolume(string parameterName, float volume)
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("Audio Mixer�� ������� �ʾҽ��ϴ�. ���� ������ ������ �� �����ϴ�.");
            return;
        }

        if (volume == 0)
        {
            masterMixer.SetFloat(parameterName, -80f); // ������ 0�� ���� -80dB (���� �Ұ�)
        }
        else
        {
            masterMixer.SetFloat(parameterName, Mathf.Log10(volume / 10f) * 20f); // 0~10 -> �α� ������ dB
        }
    }

    // Ư�� �׷� ���Ұ� ���� (���� �Ķ���ͷ� ���Ұ� ����)
    public void SetGroupMute(string parameterName, bool isMuted)
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("Audio Mixer�� ������� �ʾҽ��ϴ�. ���� ���ҰŸ� ������ �� �����ϴ�.");
            return;
        }

        // parameterName�� "BGMMute" �Ǵ� "SFXMute" ����
        string volumeParameterName = parameterName.Replace("Mute", "Volume"); // "BGMVolume" �Ǵ� "SFXVolume"

        if (isMuted)
        {
            masterMixer.SetFloat(volumeParameterName, -80f); // ���Ұ� �� -80dB�� ����
        }
        else
        {
            // ���Ұ� ���� �� PlayerPrefs�� ����� ���� ������ ����
            float savedVolume = PlayerPrefs.GetFloat(volumeParameterName, 5f); // �⺻�� 5
            SetGroupVolume(volumeParameterName, savedVolume); // SetGroupVolume�� ��Ȱ���Ͽ� dB ��ȯ ����
        }
    }

    // PlayerPrefs���� ���� ������ �ε��Ͽ� Audio Mixer�� ����
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

        Debug.Log("���� ���� �ε� �� �ͼ� ���� �Ϸ�.");
    }

    // ���� UI ������ PlayerPrefs�� ����
    public void SaveSoundSettings(float bgmVal, bool bgmMuteVal, float sfxVal, bool sfxMuteVal)
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVal);
        PlayerPrefs.SetInt("BGMMute", bgmMuteVal ? 1 : 0);
        PlayerPrefs.SetFloat("SFXVolume", sfxVal);
        PlayerPrefs.SetInt("SFXMute", sfxMuteVal ? 1 : 0);

        PlayerPrefs.Save(); // ������� ��� ����
        Debug.Log("���� ���� PlayerPrefs�� ���� �Ϸ�.");
    }
}