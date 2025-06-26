using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI ���� ����� ���� �߰�


public class ClickBtn : MonoBehaviour
{
    public MapManage mapManage;
    // �� ������ �����ϱ� ���� SpawnManager�� ���� ���۷��� �߰�
    public SpawnManager spawnManager;

    public void OnResetBtnClicked()
    {
        print("��ư Ŭ��");
        mapManage.ResetMap();
    }

    public void OnStartBtnClicked()
    {
        print("���� ����");
        // SpawnManager�� StartSpawn �޼��带 ȣ���Ͽ� �� ���� ����
        spawnManager.StartSpawningFromButton();
    }

    public void OnTileClicked()
    {
        print("Ÿ�� Ŭ��");
    }
}
