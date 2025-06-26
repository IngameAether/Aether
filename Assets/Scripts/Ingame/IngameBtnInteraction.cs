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

    public void ClickFastButton()
    {
        currentSpeed++;
        if (currentSpeed > 3) currentSpeed = 1;  // 3배속까지만 지원

        Time.timeScale = currentSpeed;

        speedTxt.text = "X" + currentSpeed.ToString();
    }

    public void ClickPauseButton()
    {
        if (!isPaused)
        {
            Time.timeScale = 0f;
            Debug.Log("일시정지 버튼 클릭");
            isPaused = true;
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("일시정지 버튼 재클릭-다시 시작");
            isPaused = false;
        }
    }
}
