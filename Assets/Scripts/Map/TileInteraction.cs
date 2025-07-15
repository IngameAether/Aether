using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInteraction : MonoBehaviour
{
    public Tile tile;
    public static GameObject[] staticElementPrefabs;  // 전역 변수로 선언(모든 tile이 공유할 내용이므로)
    public static GameObject[] staticTowerPrefabs;
    public static int clickNum = 0;  // 전체에서 클릭 횟수를 공유해야 하므로 static 선언
    public static bool isTowerJustCreated = false;  // Ÿ���� Ŭ���� Ÿ���� ��ġ�� ������ Ÿ���� Ŭ���� ������ �����ϱ� ����

    private void Start()
    {
        tile = GetComponent<Tile>();
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;   // 게임이 멈추면 클릭 등 상호작용 무시

        tile.ChangeCurrentTileColor();
        tile.PrintTileInfo();

        if (!tile.isBuild || !tile.isElementBuild) return;
        
        GameObject elementObj = null;
        int ranNum = 0; 
        
        if (clickNum == 0)
        {
            elementObj = PlacedTower(staticTowerPrefabs[0]);
            clickNum++;
            isTowerJustCreated = true;
        }
        else
        {
            ranNum = Random.Range(0, staticElementPrefabs.Length);
            elementObj = Instantiate(staticElementPrefabs[ranNum], tile.transform.position, Quaternion.identity);
            clickNum++;
        }
        
        // 원소가 배치된 타일 저장
        ElementController ec = elementObj.GetComponent<ElementController>();
        if (ec != null)
        {
            ec.Initialize(this);
            ec.selectTile = tile;
        }

        Debug.Log($"소환된 원소: {staticElementPrefabs[ranNum]}");
        tile.isElementBuild = false;
        tile.element = elementObj;
    }

    public GameObject PlacedTower(GameObject prefab)
    {
        var elementObj = Instantiate(prefab, tile.transform.position, Quaternion.identity);
        tile.isElementBuild = false;
        tile.element = elementObj;
        return elementObj;
    }
    
    public void TileReset()
    {
        tile.isElementBuild = true;
        Destroy(tile.element.gameObject);
        tile.element = null;
    }
}