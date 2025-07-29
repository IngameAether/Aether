using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Towers.Core;

public class Tile : MonoBehaviour
{
    public int x, y;   // 타일 좌표
    public bool isBuild;    // 1=타워 생성 가능한 곳, 0=적이 지나가는 경로
    public bool isElementBuild;     // true=원소 생성 가능(타일이 비어있는 경우)
    public GameObject element;
    public GameObject tower;

    private SpriteRenderer spriteRenderer;
    public Color originColor;
    public Color highlightColor = Color.black; // 타일 선택시 강조될 색상

    public ElementType CurrentLogicalElementType
    {
        get
        {
            if(tower != null) // 타워 > 원소 > 빈 타일 순으로 우선시
            {
                return ElementType.Tower;
            }
            else if(element != null) 
            {
                ElementController ec = element.GetComponent<ElementController>();
                if (ec != null)
                {
                    return ec.type;
                }
            }
            return ElementType.None;
        }
    }

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
        ApplyHighlight(false); // 초기 상태는 하이라이트 안함
    }

    void GetTileObject()
    {
        Debug.Log(element, tower);
    }

    public void ApplyHighlight(bool isSelected)
    {
        if(spriteRenderer != null)
        {
            spriteRenderer.color = isSelected ? highlightColor : originColor;
        }
    }

    public void PrintTileInfo()
    {
        Debug.Log($"타일 좌표:({x},{y}), 설치 가능:{isBuild}, 원소 설치 여부:{isElementBuild}");
    }
}
