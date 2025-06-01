using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickBtn : MonoBehaviour
{
    public MapManage mapManage;

    public void OnResetBtnClicked()
    {
        mapManage.ResetMap();
    }
}
