using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVLoad : MonoBehaviour
{
    void Awake()
    {
        List<Dictionary<string, object>> enemyData = CSVReader.Read("Enemy_Data");
        //List<Dictionary<string, object>> towerData = CSVReader.Read("Tower_Data");

        //for (int i=0; i<enemyData.Count; i++)
        //{
        //    print(enemyData[i]["identification_code"].ToString());
        //}
    }
}
