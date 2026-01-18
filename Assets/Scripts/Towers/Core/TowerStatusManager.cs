using UnityEngine;

[System.Obsolete("TowerStatManager is deprecated. Use MagicBookBuffSystem and Tower classes instead.")]
public class TowerStatManager : MonoBehaviour
{
    public static TowerStatManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
