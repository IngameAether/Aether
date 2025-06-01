using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    public GameObject pathTile;  // 적이 지나다니는 통로 타일
    public GameObject mapTile;   // 타워 배치할 수 있는(벽) 타일
    GameObject[,] tileObjects;   // 타일 참조할 수 있게 저장할 배열
    bool isBuild;   // 배치 가능한 타일인지 확인용
    public GameObject[] elementPrefabs;   // 원소 프리팹 담을 배열
    public Transform parent;

    public void RenderMap(int[,] mapTiles)
    {
<<<<<<< HEAD:Assets/Scripts/Map/MapRenderer.cs
        // 기존 타일 오브젝트가 있다면 모두 제거
        ClearMap();
=======
        TileInteraction.staticElementPrefabs = elementPrefabs;  // 전역 변수에 원소 프리팹 배열 저장
>>>>>>> Test:Assets/Scripts/MapRenderer.cs

        tileObjects = new GameObject[mapTiles.GetLength(0), mapTiles.GetLength(1)];

        float setx = (mapTiles.GetLength(0) - 1) / 2;
        float sety = (mapTiles.GetLength(1) - 1) / 2;

        for (int x = 0; x < mapTiles.GetLength(0); x++)
        {
            for (int y = 0; y < mapTiles.GetLength(1); y++)
            {
                isBuild = (mapTiles[x, y] == 0);

                GameObject prefab = mapTiles[x, y] == 1 ? pathTile : mapTile;
<<<<<<< HEAD:Assets/Scripts/Map/MapRenderer.cs
                Vector2 position = new Vector2(y - setx, -x + sety);
                GameObject tileObject = Instantiate(prefab, position, Quaternion.identity);
=======
                Vector2 position = new Vector2(y-setx,-x+sety-0.5f);
                GameObject tileObject = Instantiate(prefab, position, Quaternion.identity,parent);   // 타일 prefab 생성
>>>>>>> Test:Assets/Scripts/MapRenderer.cs

                tileObject.GetComponent<Tile>().Initialize(x, y, isBuild);  // 각 타일의 정보를 기억하기 위해
                tileObjects[x, y] = tileObject;
            }
        }
    }

    public void ClearMap()
    {
        if (tileObjects == null) return;
        for (int x = 0; x < tileObjects.GetLength(0); x++)
        {
            for (int y = 0; y < tileObjects.GetLength(1); y++)
            {
                if (tileObjects[x, y] != null)
                    Destroy(tileObjects[x, y]);
            }
        }
    tileObjects = null; // 배열 초기화
    Debug.Log("기존 맵 오브젝트 제거 완료.");
    }

public Vector3 GetTileWorldPosition(int tileX, int tileY, int mapWidth, int mapHeight)
{
        float offsetX = (mapWidth - 1) / 2.0f;
        float offsetY = (mapHeight - 1) / 2.0f;

        // RenderMap에서 사용된 타일 기준점 위치 계산
        float baseX = tileY - offsetX;
        float baseY = -tileX + offsetY;

        // 타일 크기가 1x1 유닛이므로 타일 중앙 좌표 계산 시 0.5f 오프셋 적용
        float tileCenterX = baseX + 0.5f;
        float tileCenterY = baseY + 0.5f - 1.0f;

        float tileZ = 0f; // 또는 필요한 Z 값

        return new Vector3(tileCenterX, tileCenterY, tileZ);
    }
}