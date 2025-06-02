using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;   // Ÿ�� ��ǥ
    public bool isBuild;    // 1=Ÿ�� ���� ������ ��, 0=���� �������� ���
    public bool isElementBuild;
    public GameObject element;
    public GameObject tower;

    private SpriteRenderer spriteRenderer;
    public Color originColor;
    static Tile currentTile = null;    // ���� ���õ� Ÿ��

    // �ʱ�ȭ
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
        if (currentTile != null && currentTile != this)   // �ٸ� Ÿ�� �����ϸ� ���� Ÿ�� �� ������� �ǵ���
            currentTile.spriteRenderer.color = originColor;

        currentTile = this;
        spriteRenderer.color = originColor * 1.5f;
    }

    public void PrintTileInfo()
    {
        Debug.Log($"Ÿ�� ��ǥ:({x},{y}), ��ġ ����:{isBuild}, ���� ��ġ ����:{isElementBuild}");
    }
}
