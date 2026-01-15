using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInfoData
{
    public string IdCode;
    public string Hp;
    public float Speed;
    public string DamageReduction;
    public int Aether;
    public int Element;
    public int SlowdownGauge;
    public int StunGauge;
    public int BurnGauge;
    public int BleedingGauge;
    public string Description;

    public EnemyInfoData(Dictionary<string, object> dic)
    {
        IdCode = dic["identification_code"].ToString();
        Hp = dic["hp"].ToString();
        Speed = Convert.ToSingle(dic["speed"]);
        DamageReduction = dic["damage_reduction_level"].ToString();
        Aether = (int)dic["aether"];
        Element = (int)dic["light/dark_element"];
        SlowdownGauge = (int)dic["slowdown_gauge"];
        StunGauge = (int)dic["stun_gauge"];
        BurnGauge = (int)dic["burn_gauge"];
        BleedingGauge = (int)dic["bleeding_gauge"];
        Description = dic["special_ability"].ToString();
    }
}
