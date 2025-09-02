using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SplitOnSpawn", menuName = "TowerDefense/Ability/SplitOnSpawn")]
public class SplitOnSpawn : SpecialAbility
{
    private int splitCount = 2;
    public override void ApplySpecialAbility(NormalEnemy enemy)
    {
        SpawnManager spawnManager = enemy.GetComponent<SpawnManager>();
        if (spawnManager != null )
        {
            Vector3 spawnPos = enemy.transform.position;

            for (int i=1; i<splitCount; i++)
            {
                spawnManager.SpawnSplitEnemy(spawnPos, enemy.curEnemyIndex);
            }
        }
    }
}
