using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    public void Preload(GameObject prefab, int count)
    {
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            pools[prefab].Enqueue(go);
        }
    }

    public GameObject Get(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();
        if (pools[prefab].Count == 0)
        {
            var inst = Instantiate(prefab, transform);
            return inst;
        }
        else
        {
            var go = pools[prefab].Dequeue();
            go.SetActive(true);
            return go;
        }
    }

    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        var prefab = obj; // 이 예시는 prefab 키를 알 수 없으므로 단순화. 실제로는 prefab을 연결/추적 필요.
        // 권장: ProjectileController에서 Release API로 관리하거나, 풀에서 prefab-to-instance map 유지
        Destroy(obj); // 기본 예시: 인스펙터에서 풀을 확장하지 않으면 Destroy 처리
    }
}
