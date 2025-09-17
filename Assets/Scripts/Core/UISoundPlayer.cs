using UnityEngine;

public class UISoundPlayer : MonoBehaviour
{
    // 버튼 클릭 효과음을 재생하는 단일 함수
    public void PlayClickSound()
    {
        // 프로젝트의 AudioManager를 호출하여 기본 버튼 클릭 사운드를 재생합니다.
        // SfxType.ButtonClick 등 미리 정해둔 효과음 이름을 사용합니다.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SfxType.Screen_touch);
        }
    }

    // (선택) 다른 종류의 사운드가 필요하면 함수를 추가할 수 있습니다.
    //public void PlayCancelSound()
    //{
    //    if (AudioManager.Instance != null)
    //    {
    //        AudioManager.Instance.PlaySFX(SfxType.ButtonCancel);
    //    }
    //}
}
