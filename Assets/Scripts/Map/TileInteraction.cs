using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInteraction : MonoBehaviour
{
    Tile tile;
    public static GameObject[] staticElementPrefabs;  // 전역 변수로 선언(모든 tile이 공유할 내용이므로)

    void Start()
    {
        tile = GetComponent<Tile>();
    }

    private void OnMouseDown()
    {
        tile.ChangeCurrentTileColor();
        tile.PrintTileInfo();

        if (!tile.isBuild || !tile.isElementBuild) return;

        int ranNum = Random.Range(0, staticElementPrefabs.Length);
        Instantiate(staticElementPrefabs[ranNum], tile.transform.position, Quaternion.identity);
        Debug.Log($"소환된 원소: {staticElementPrefabs[ranNum]}");
        tile.isElementBuild = false;
    }
}
