using UnityEngine;

public class MapBackgroundManager : MonoBehaviour
{
    [Header("배경 오브젝트")]
    [SerializeField] private GameObject background_Spring;
    [SerializeField] private GameObject background_Forest;
    [SerializeField] private GameObject background_WinterForest;
    [SerializeField] private GameObject background_DeathForest;

    /// <summary>
    /// 현재 웨이브에 맞춰 올바른 배경을 활성화합니다.
    /// </summary>
    public void UpdateBackground(int currentWave)
    {
        // 0~24 웨이브: 봄
        if (currentWave < 25)
        {
            SetActiveBackground(background_Spring);
        }
        // 25~49 웨이브: 여름
        else if (currentWave < 50)
        {
            SetActiveBackground(background_Forest);
        }
        // 50~74 웨이브: 가을
        else if (currentWave < 75)
        {
            SetActiveBackground(background_WinterForest);
        }
        // 75 웨이브 이상: 겨울
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
