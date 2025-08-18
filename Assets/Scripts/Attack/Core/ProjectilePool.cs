using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> pools = new();
    private Dictionary<GameObject, GameObject> prefabLookup = new(); // instance → prefab 매핑

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 특정 prefab을 미리 풀에 채워넣기
    /// </summary>
    public void Preload(GameObject prefab, int count)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            pools[prefab].Enqueue(go);
            prefabLookup[go] = prefab;
        }
    }

    /// <summary>
    /// 풀에서 오브젝트 가져오기 (없으면 새로 생성)
    /// </summary>
    public GameObject Get(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        GameObject go;
        if (pools[prefab].Count > 0)
        {
            go = pools[prefab].Dequeue();
        }
        else
        {
            go = Instantiate(prefab, transform);
            prefabLookup[go] = prefab;
        }

        go.SetActive(true);
        return go;
    }

    /// <summary>
    /// 사용 끝난 오브젝트를 풀로 반환
    /// </summary>
    public void Release(GameObject obj)
    {
        if (prefabLookup.TryGetValue(obj, out var prefab))
        {
            obj.SetActive(false);
            pools[prefab].Enqueue(obj);
        }
        else
        {
            Debug.LogWarning($"[ProjectilePool] 풀에 없는 오브젝트 {obj.name} 이므로 Destroy 처리");
            Destroy(obj);
        }
    }
}
