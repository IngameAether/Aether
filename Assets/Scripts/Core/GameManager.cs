using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using UnityEngine.Rendering.PostProcessing; 
using System.Collections;

public enum EScene
{
    MainMenu,
    InGame,
    // 필요한 다른 씬 추가
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(gameObject);
    }

    public void OnStartGame()
    {
        ChangeScene(EScene.InGame);
    }

    public void ChangeScene(EScene scene)
    {
        SceneManager.LoadScene((int)scene);
    }
    
}