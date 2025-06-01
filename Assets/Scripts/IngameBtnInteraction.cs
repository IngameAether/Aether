using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameBtnInteraction : MonoBehaviour
{
    public TextMeshProUGUI speedTxt;
    int currentSpeed = 1;

    public void ClickFastButton()
    {
        currentSpeed++;
        if (currentSpeed > 3) currentSpeed = 1;  // 3��ӱ����� ����

        Time.timeScale = currentSpeed;

        speedTxt.text = "X" + currentSpeed.ToString();
    }

    public void ClickPauseButton()
    {
        Time.timeScale = 0f;
    }
}
