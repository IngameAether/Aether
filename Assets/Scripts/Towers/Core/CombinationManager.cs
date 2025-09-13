using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombinationManager : MonoBehaviour
{
    // 싱글톤 설정
    public static CombinationManager Instance { get; private set; }

    [Header("모든 조합법 리스트")]
    [SerializeField] private List<CombinationRecipe> recipes;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 조합을 시도하는 메인 함수
    public void TryCombineTowers(List<Tower> towersToCombine)
    {
        // 조합을 시도하는 타워가 3개가 아니면 실패
        if (towersToCombine.Count != 3) return;

        // 조합할 타워들의 ID를 리스트로 만들고, 정렬 (순서에 상관없이 비교하기 위해)
        List<string> inputIds = towersToCombine.Select(t => t.GetTowerData().ID).ToList();
        inputIds.Sort();

        // 모든 레시피를 확인하며 일치하는 조합법이 있는지 찾기
        foreach (var recipe in recipes)
        {
            if (recipe.requiredTowerIds.Count != 3) continue;

            List<string> recipeIds = new List<string>(recipe.requiredTowerIds);
            recipeIds.Sort();

            // 정렬된 ID 리스트가 정확히 일치하는지 확인
            if (inputIds.SequenceEqual(recipeIds))
            {
                // 조합 성공!
                PerformCombination(towersToCombine, recipe.resultingTowerData);
                return; // 조합을 마쳤으니 함수 종료
            }
        }

        // 일치하는 레시피를 찾지 못함
        Debug.Log("조합에 실패했습니다.");
    }

    private void PerformCombination(List<Tower> towersToCombine, TowerData resultData)
    {
        // 조합의 중심이 될 타워의 위치를 정함 (예: 첫 번째 타워 위치)
        Vector3 spawnPosition = towersToCombine[0].transform.position;

        // 조합에 사용된 타워들 삭제
        foreach (var tower in towersToCombine)
        {
            Destroy(tower.gameObject);
        }

        // 결과 타워 생성 (기본 Tower 프리팹 사용)
        // 게임에 맞는 기본 Tower 프리팹을 로드하거나 미리 연결해두어야 합니다.
        GameObject basicTowerPrefab = Resources.Load<GameObject>("Prefabs/Tower"); // 예시
        GameObject newTowerObject = Instantiate(basicTowerPrefab, spawnPosition, Quaternion.identity);

        // 생성된 타워에 결과 데이터(resultingTowerData)를 설정
        newTowerObject.GetComponent<Tower>().Setup(resultData);

        Debug.Log($"조합 성공! {resultData.Name} 생성!");
    }
}
