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
    public string ControlResistance;
    public int Aether;
    public int ReinforceElement;
    public int SlowdownGauge;
    public int BurnGauge;
    public int StunGauge;
    public int BleedingGauge;
    public string Description;

    public EnemyInfoData(Dictionary<string, object> dic)
    {
        IdCode = dic["identification_code"].ToString();
        Hp = dic["hp"].ToString();
        Speed = Convert.ToSingle(dic["speed"]);
        DamageReduction = dic["damage_reduction_level"].ToString();
        ControlResistance = dic["control_resistance_level"].ToString();
        Aether = (int)dic["aether"];
        ReinforceElement = (int)dic["light/dark_element"];
        SlowdownGauge = (int)dic["slowdown_gauge"];
        BurnGauge = (int)dic["burn_gauge"];
        StunGauge = (int)dic["stun_gauge"];
        BleedingGauge = (int)dic["bleeding_gauge"];
        Description = dic["special_ability"].ToString();
    }
}
