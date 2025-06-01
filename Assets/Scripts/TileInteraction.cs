using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInteraction : MonoBehaviour
{
    Tile tile;
    public static GameObject[] staticElementPrefabs;  // ���� ������ ����(��� tile�� ������ �����̹Ƿ�)

    void Start()
    {
        tile = GetComponent<Tile>();
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;   // ������ ���߸� Ŭ�� �� ��ȣ�ۿ� ����

        tile.ChangeCurrentTileColor();
        tile.PrintTileInfo();

        if (!tile.isBuild || !tile.isElementBuild) return;

        int ranNum = Random.Range(0, staticElementPrefabs.Length);
        Instantiate(staticElementPrefabs[ranNum], tile.transform.position, Quaternion.identity);
        Debug.Log($"��ȯ�� ����: {staticElementPrefabs[ranNum]}");
        tile.isElementBuild = false;
    }
}
