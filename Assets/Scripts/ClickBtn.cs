using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickBtn : MonoBehaviour
{
    public MapManage mapManage;

    public void OnResetBtnClicked()
    {
        print("��ư Ŭ��");
        mapManage.ResetMap();
    }

    public void OnTileClicked()
    {
        print("Ÿ�� Ŭ��");
    }
}
