using UnityEngine;

public class TowerExtension : MonoBehaviour
{
    // --- 참조 ---
    private Tower _tower; // 원본 Tower 스크립트 참조

    // --- 상태 변수 ---
    private int _lightReinforceCount = 0;
    private int _darkReinforceCount = 0;

    // --- 추가 능력치(버프) 변수들 ---
    private float _bonusRange = 0f;
    private float _bonusBuildup = 0f;
    private float _bonusEffectDuration = 0f;
    private float _bonusEffectValue = 0f; // 상태이상 효과 값 보너스
    // TODO: 실제 버프 시스템과 연동할 변수들 (임시)
    private float _bonusDamageMultiplier = 1.0f;
    private float _bonusAttackSpeedMultiplier = 1.0f;

    // --- 최종 능력치를 계산하는 프로퍼티 ---
    public float BuffedRange => _tower.Range + _bonusRange;
    public float BuffedDamage => _tower.Damage * _bonusDamageMultiplier;
    public float BuffedAttackSpeed => _tower.AttackSpeed * _bonusAttackSpeedMultiplier;
    public float BuffedEffectBuildup => _tower.towerData.effectBuildup + _bonusBuildup;
    public float BuffedEffectDuration => _tower.towerData.effectDuration + _bonusEffectDuration;
    public float BuffedEffectValue => _tower.towerData.effectValue + _bonusEffectValue;

    private void Awake()
    {
        // 자기 자신의 Tower 컴포넌트를 먼저 찾아 연결합니다.
        _tower = GetComponent<Tower>();

        if (_tower == null)
        {
            Debug.LogError("TowerExtension: 같은 오브젝트에서 Tower.cs를 찾을 수 없습니다!");
        }
    }

    // --- 외부 시스템(TowerStatManager 등)이 호출할 함수들 ---
    public void AddBonusRange(float amount) { _bonusRange += amount; }
    public void AddBonusBuildup(float amount) { _bonusBuildup += amount; }
    public void AddBonusEffectDuration(float amount) { _bonusEffectDuration += amount; }
 
    // --- 강화 및 진화 로직 ---
    public void Reinforce(ReinforceType type)
    {
        var towerData = _tower.towerData;
        if (type == ReinforceType.Light)
        {
            _lightReinforceCount++;
            Debug.Log($"{towerData.Name} Light 강화! ({_lightReinforceCount}/{towerData.reinforcementThreshold})");
            if (towerData.lightEvolutionData != null && _lightReinforceCount >= towerData.reinforcementThreshold)
            {
                Evolve(towerData.lightEvolutionData);
            }
        }
        else if (type == ReinforceType.Dark)
        {
            _darkReinforceCount++;
            Debug.Log($"{towerData.Name} Dark 강화! ({_darkReinforceCount}/{towerData.reinforcementThreshold})");
            if (towerData.darkEvolutionData != null && _darkReinforceCount >= towerData.reinforcementThreshold)
            {
                Evolve(towerData.darkEvolutionData);
            }
        }
    }

    private void Evolve(TowerData evolutionData)
    {
        Debug.Log($"{_tower.towerData.Name}이(가) {evolutionData.Name}(으)로 진화합니다!");
        GameObject prefabToSpawn = evolutionData.upgradedPrefab != null ? evolutionData.upgradedPrefab : this.gameObject;
        GameObject newTowerObject = Instantiate(prefabToSpawn, transform.position, transform.rotation);
        newTowerObject.GetComponent<Tower>()?.Setup(evolutionData);
        Destroy(gameObject);
    }
}
