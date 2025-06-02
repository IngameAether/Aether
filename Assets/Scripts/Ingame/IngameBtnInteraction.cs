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
        if (currentSpeed > 3) currentSpeed = 1;  // 3��ӱ����� ����

        Time.timeScale = currentSpeed;

        speedTxt.text = "X" + currentSpeed.ToString();
    }

    public void ClickPauseButton()
    {
        if (!isPaused)
        {
            Time.timeScale = 0f;
            Debug.Log("�Ͻ����� ��ư Ŭ��");
            isPaused = true;
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("�Ͻ����� ��ư ��Ŭ��-�ٽ� ����");
            isPaused = false;
        }
    }
}
