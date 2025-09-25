using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TowerInfoData
{
    public string Name;
    public string IdCode;
    public string Combination;
    public string Attack;
    public float Range;
    public float Speed;
    public string CriticalRate;
    public int UnleashingPotential;
    public int MaxReinforcement;

    public TowerInfoData(Dictionary<string, object> dic)
    {
        Name = dic["image"].ToString();
        IdCode = dic["id_code"].ToString();
        Combination = dic["combination"].ToString();
        Attack = dic["attack"].ToString();
        Range = Convert.ToSingle(dic["range"]);
        Speed = Convert.ToSingle(dic["speed"]);
        CriticalRate = dic["critical_rate"].ToString();
        UnleashingPotential = (int)dic["unleashing_potential"];
        MaxReinforcement = (int)dic["max_reinforcement"];
    }
}
