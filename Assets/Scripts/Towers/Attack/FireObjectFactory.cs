using UnityEngine;

public static class FireObjectFactory
{
    public static T Spawn<T>(T prefab, Vector2 towerPos, Transform target)
        where T : FireObjectBase
    {
        T obj = Object.Instantiate(prefab);
        obj.Init(towerPos, target);
        return obj;
    }
}
