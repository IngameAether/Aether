using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public GameObject SettingBtn;

    public void OpenSettings()
    {
        Time.timeScale = 0f;
        SettingBtn.SetActive(true);
    }

    public void CloseSettings()
    {
        Time.timeScale = 1f;
        SettingBtn.SetActive(false);
    }

    public void ChangeMainMenuScene()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
