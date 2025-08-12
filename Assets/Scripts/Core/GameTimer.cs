using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("UI 설정")]
    public TextMeshProUGUI timeDisplayText;
    private float _totalGameTimeInSeconds = 0f; // 누적된 게임 시간
    private bool _isTimeRunning = false; // 시간이 현재 흐르고 있는지 확인

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }

    void Update()
    {
        if (!_isTimeRunning)
        {
            return; // 시간이 흐르지 않으면 업데이트 중단
        }
        // 1. 누적 게임 시간을 업데이트
        _totalGameTimeInSeconds += Time.deltaTime;

        // 2. 누적된 시간은 분/초로 나눔
        int minutes = Mathf.FloorToInt(_totalGameTimeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(_totalGameTimeInSeconds % 60f);

        // 3. 99분을 초과하는 경우
        if(minutes > 99)
        {
            minutes = 99;
            seconds = 59;
            /* 정해진 시간을 넘기면 게임오버?
             * stopTimer();
             * Game.Manager.Instance.GameOver();
             */
        }

        // 4. 시간을 00:00로 보이도록
        string formattedTime = $"{minutes:00}:{seconds:00}";

        // 5. UI 텍스트 컴포넌트에 포맷된 시간을 표시
        if(timeDisplayText != null)
        {
            timeDisplayText.text = formattedTime;
        }
    }

    public void StartTimer()
    {
        _isTimeRunning = true;
        Debug.Log("게임 타이머 시작");
    }

    public void StopTimer()
    {
        _isTimeRunning = false;
    }

    public void ResetGameTime()
    {
        _totalGameTimeInSeconds = 0f;
        _isTimeRunning = false;
        if(timeDisplayText != null)
        {
            timeDisplayText.text = "00:00";
        }
        Debug.Log("게임 시간 00:00로 초기화");
    }

    public float GetCurrentGameTimeInSeconds()
    {
        return _totalGameTimeInSeconds;
    }
}
