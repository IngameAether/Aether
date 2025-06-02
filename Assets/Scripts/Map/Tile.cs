using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;   // 타일 좌표
    public bool isBuild;    // 1=타워 생성 가능한 곳, 0=적이 지나가는 경로
    public bool isElementBuild;
    public GameObject element;
    public GameObject tower;

    private SpriteRenderer spriteRenderer;
    public Color originColor;
    static Tile currentTile = null;    // 현재 선택된 타일

    // 초기화
    public void Initialize(int x, int y, bool isBuild)
    {
        this.x = x;
        this.y = y;
        this.isBuild = isBuild;
        if (isBuild) isElementBuild = true;
        element = null;
        tower = null;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originColor = spriteRenderer.color;
        }
    }

    void GetTileObject()
    {
        Debug.Log(element, tower);
    }

    public void ChangeCurrentTileColor()
    {
        if (currentTile != null && currentTile != this)   // 다른 타일 선택하면 이전 타일 색 원래대로 되돌림
            currentTile.spriteRenderer.color = originColor;

        currentTile = this;
        spriteRenderer.color = originColor * 1.5f;
    }

    public void PrintTileInfo()
    {
        Debug.Log($"타일 좌표:({x},{y}), 설치 가능:{isBuild}, 원소 설치 여부:{isElementBuild}");
    }
}
