using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;   // 타일 좌표
    public bool isBuild;    // 1=타워 생성 가능한 곳, 0=적이 지나가는 경로
    public GameObject element;
    public GameObject tower;

    // 초기화
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
