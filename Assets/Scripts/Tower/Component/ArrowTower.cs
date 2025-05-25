using UnityEngine;

/// <summary>
/// 테스트용 타워
/// </summary>
public class ArrowTower : Tower
{
    protected override void Attack()
    {
        Debug.Log("ArrowTower: Attack !");       
    }
}