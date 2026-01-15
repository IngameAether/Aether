using UnityEngine;
using System; // Enum.Parse를 쓰기 위해 필요

public class UISoundPlayer : MonoBehaviour
{
    // 기본 클릭 소리
    public void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SfxType.Screen_touch);
        }
    }

    // 외부에서 이름(String)으로 소리를 지정하는 만능 함수
    public void PlaySoundByName(string soundName)
    {
        if (AudioManager.Instance == null) return;

        try
        {
            // 입력받은 문자열(soundName)을 SfxType(Enum)으로 변환합니다.
            SfxType type = (SfxType)Enum.Parse(typeof(SfxType), soundName);

            AudioManager.Instance.PlaySFX(type);
        }
        catch
        {
            // 오타가 났을 경우 오류 로그 출력
            Debug.LogError($"[UISoundPlayer] '{soundName}'라는 이름의 SfxType을 찾을 수 없습니다! 스펠링을 확인하세요.");
        }
    }
}
