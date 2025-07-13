using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInteraction : MonoBehaviour
{
    Tile tile;
    public static GameObject[] staticElementPrefabs;  // 전역 변수로 선언(모든 tile이 공유할 내용이므로)
    public static GameObject[] staticTowerPrefabs;
    public static int clickNum = 0;  // 전체에서 클릭 횟수를 공유해야 하므로 static 선언
    public static bool isTowerJustCreated = false;  // 타일을 클릭해 타워가 배치된 것인지 타워를 클릭한 것인지 구분하기 위해

    void Start()
    {
        tile = GetComponent<Tile>();
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;   // 게임이 멈추면 클릭 등 상호작용 무시

        tile.ChangeCurrentTileColor();
        tile.PrintTileInfo();

        if (!tile.isBuild || !tile.isElementBuild) return;

        // 타일 첫 클릭은 타워 배치되게: 나중에 되돌릴 예정
        int ranNum = 0; GameObject elementObj = null;
        if (clickNum == 0)
        {
            elementObj = Instantiate(staticTowerPrefabs[0], tile.transform.position, Quaternion.identity);
            clickNum++;
            isTowerJustCreated = true;
        }
        else
        {
            ranNum = Random.Range(0, staticElementPrefabs.Length);
            elementObj = Instantiate(staticElementPrefabs[ranNum], tile.transform.position, Quaternion.identity);
            clickNum++;
        }
        //int ranNum = Random.Range(0, staticElementPrefabs.Length);
        //GameObject elementObj = Instantiate(staticElementPrefabs[ranNum], tile.transform.position, Quaternion.identity);

        // 원소가 배치된 타일 저장
        ElementController ec = elementObj.GetComponent<ElementController>();
        if (ec != null) ec.selectTile = tile;

        Debug.Log($"소환된 원소: {staticElementPrefabs[ranNum]}");
        tile.isElementBuild = false;
        tile.element = elementObj;
    }
}
