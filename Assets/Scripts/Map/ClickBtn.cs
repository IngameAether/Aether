using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 관련 기능을 위해 추가


public class ClickBtn : MonoBehaviour
{
    public MapManage mapManage;
    // 적 스폰을 시작하기 위해 SpawnManager에 대한 레퍼런스 추가
    public SpawnManager spawnManager;

    public void OnResetBtnClicked()
    {
        print("버튼 클릭");
        mapManage.ResetMap();
    }

    public void OnStartBtnClicked()
    {
        print("게임 시작");
        // SpawnManager의 StartSpawn 메서드를 호출하여 적 스폰 시작
        spawnManager.StartSpawningFromButton();
    }

    public void OnTileClicked()
    {
        print("타일 클릭");
    }
}
