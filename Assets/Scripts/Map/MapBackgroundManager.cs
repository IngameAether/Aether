using UnityEngine;

public class MapBackgroundManager : MonoBehaviour
{
    [Header("배경 오브젝트")]
    [SerializeField] private GameObject background_Spring;
    [SerializeField] private GameObject background_Forest;
    [SerializeField] private GameObject background_WinterForest;
    [SerializeField] private GameObject background_DeathForest;

    private void Start()
    {
        // GameManager에서 현재 웨이브 정보를 가져옵니다.
        // 만약 GameManager가 아직 준비되지 않았거나 웨이브가 0이라면 1(봄)로 설정합니다.
        int startWave = 1;
        if (GameManager.Instance != null && GameManager.Instance.CurrentWave > 0)
        {
            startWave = GameManager.Instance.CurrentWave;
        }

        // 즉시 배경 업데이트
        UpdateBackground(startWave);
    }

    // 현재 웨이브에 맞춰 올바른 배경을 활성화합니다.
    public void UpdateBackground(int currentWave)
    {
        // 1~25 웨이브: 봄
        if (currentWave < 26)
        {
            SetActiveBackground(background_Spring);
        }
        // 25~50 웨이브: 여름
        else if (currentWave < 51)
        {
            SetActiveBackground(background_Forest);
        }
        // 50~75 웨이브: 가을
        else if (currentWave < 76)
        {
            SetActiveBackground(background_WinterForest);
        }
        // 76 웨이브 이상: 겨울
        else
        {
            SetActiveBackground(background_DeathForest);
        }
    }

    // 하나의 배경만 켜고 나머지는 모두 끄는 함수
    private void SetActiveBackground(GameObject activeBg)
    {
        background_Spring.SetActive(activeBg == background_Spring);
        background_Forest.SetActive(activeBg == background_Forest);
        background_WinterForest.SetActive(activeBg == background_WinterForest);
        background_DeathForest.SetActive(activeBg == background_DeathForest);
    }
}
