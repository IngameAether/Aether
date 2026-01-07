using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThankYou : MonoBehaviour
{
    private void OnEnable()
    {
        Invoke("GoToMain", 2f);
    }
    private void GoToMain()
    {
        // 팝업을 닫는 명령을 추가합니다.
        if (PopUpManager.Instance != null)
        {
            PopUpManager.Instance.CloseCurrentPopUp();
        }

        // 팝업이 열려있어서 멈춘 시간을 다시 흐르게 합니다.
        Time.timeScale = 0.5f;

        // FadeManager를 통해 부드럽게 메인 메뉴로 이동합니다.
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.TransitionToScene("MainMenuScene");
        }
        else
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
