using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickBtn : MonoBehaviour
{
    public MapManage mapManage;

    public void OnResetBtnClicked()
    {
        print("버튼 클릭");
        mapManage.ResetMap();
    }

    public void OnTileClicked()
    {
        print("타일 클릭");
    }
}
