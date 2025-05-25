using UnityEngine;
using UnityEngine.SceneManagement;

public enum EScene
{
    MainMenu,
    InGame,
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

    // 테스트용
    private void Start()
    {
        ChangeScene(EScene.InGame);
    }

    public void ChangeScene(EScene scene)
    {
        SceneManager.LoadScene((int)scene);
    }
    
}