using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;   // Ÿ�� ��ǥ
    public bool isBuild;    // 1=Ÿ�� ���� ������ ��, 0=���� �������� ���
    public GameObject element;
    public GameObject tower;

    // �ʱ�ȭ
    public void Initialize(int x, int y, bool isBuild)
    {
        this.x = x;
        this.y = y;
        this.isBuild = isBuild;
        element = null;
        tower = null;
    }

    void GetTileObject()
    {
        Debug.Log(element, tower);
    }
}
