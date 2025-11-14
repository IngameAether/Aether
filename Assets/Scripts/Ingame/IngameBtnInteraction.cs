using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameBtnInteraction : MonoBehaviour
{
    public TextMeshProUGUI speedTxt;
    int currentSpeed = 1;
    bool isPaused = false;

    // 일시정지 직전의 속도를 기억할 변수
    private float _speedBeforePause = 1f;

    [Header("Pause Button UI")]
    [SerializeField] private Image pauseButtonImage; // 일시정지 버튼의 Image 컴포넌트
    [SerializeField] private Sprite pauseSprite;     // 일시정지 상태 (|| 아이콘) 스프라이트
    [SerializeField] private Sprite playSprite;      // 재생 상태 (>) 아이콘 스프라이트

    public void ClickFastButton()
    {
        // 일시정지 상태에서는 배속 버튼이 작동하지 않도록 방지
        if (isPaused) return;

        currentSpeed++;
        if (currentSpeed > 3) currentSpeed = 1;  // 3배속까지만 지원

        Time.timeScale = currentSpeed;

        // 현재 배속을 _speedBeforePause에도 저장
        _speedBeforePause = currentSpeed;

        speedTxt.text = "X" + currentSpeed.ToString();
    }

    public void ClickPauseButton()
    {
        if (!isPaused)
        {
            // 게임을 멈추기 직전의 속도를 저장
            _speedBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            Debug.Log("일시정지 버튼 클릭");
            isPaused = true;

            if (pauseButtonImage != null && playSprite != null)
            {
                pauseButtonImage.sprite = playSprite;
            }
        }
        else
        {
            Time.timeScale = _speedBeforePause;
            Debug.Log("일시정지 버튼 재클릭-다시 시작");
            isPaused = false;

            if (pauseButtonImage != null && pauseSprite != null)
            {
                pauseButtonImage.sprite = pauseSprite;
            }
        }
    }
}
