using System.Collections.Generic;
using UnityEngine;

namespace Towers.Core
{
    public enum ElementType
    {
        Fire,       // 불 원소
        Water,      // 물 원소  
        Lightning,  // 번개 원소
        None        // 원소 없음
    }
    
    // 원소 뭐 있는지 몰라서 생각나는거 다 적음
    
    public class TowerCombiner : MonoBehaviour
    {
        [Header("타워 조합 설정")] 
        [SerializeField] private GameObject arrowTowerPrefab;
        [SerializeField] private Transform towerParent;

        [Header("조합 결과 설정")] 
        [SerializeField] private Vector3 spawnPosition = Vector3.zero;
        
        private readonly List<ElementType> _selectedElements = new List<ElementType>();
        
        private Dictionary<ElementType, GameObject> _elementTowerMap;

        private void Start()
        {
            InitializeTowerMapping();
        }

        /// <summary>
        ///     원소별 타워 매핑을 초기화합니다
        ///     현재는 모든 원소가 ArrowTower로 설정되어 있습니다.
        /// </summary>
        private void InitializeTowerMapping()
        {
            _elementTowerMap = new Dictionary<ElementType, GameObject>
            {
                { ElementType.Fire, arrowTowerPrefab },
                { ElementType.Water, arrowTowerPrefab },
                { ElementType.Lightning, arrowTowerPrefab },
            };
        }

        /// <summary>
        ///     원소를 선택하는 함수 (클릭 이벤트에서 호출됩니다)
        ///     최대 3개까지 선택할 수 있습니다
        /// </summary>
        /// <param name="elementType">선택할 원소 타입</param>
        public void SelectElement(ElementType elementType)
        {
            if (_selectedElements.Count >= 3)
            {
                Debug.LogWarning("이미 3개의 원소가 선택되었습니다. 조합을 시도하거나 초기화하세요.");
                return;
            }
            
            if (elementType == ElementType.None)
            {
                Debug.LogWarning("유효하지 않은 원소입니다.");
                return;
            }

            _selectedElements.Add(elementType);
            Debug.Log($"{elementType} 원소가 선택되었습니다. 현재 선택된 원소 수: {_selectedElements.Count}/3");
            
            if (_selectedElements.Count == 3) TryTowerCombination();
        }

        /// <summary>
        ///     타워 조합을 시도하는 함수
        ///     선택된 3개의 원소가 모두 같은지 확인하고 타워를 생성합니다
        /// </summary>
        public void TryTowerCombination()
        {
            ElementType element1 = _selectedElements[0];
            ElementType element2 = _selectedElements[1];
            ElementType element3 = _selectedElements[2];
            
            if (element1 == element2 && element2 == element3)
            {
                Debug.Log($"조합 성공! {element1} 원소 3개가 조합되어 1단계 타워가 생성됩니다.");
                CreateLevel1Tower(element1);
            }
            else
            {
                Debug.Log($"조합 실패! 같은 원소 3개가 아닙니다. 선택된 원소: {element1}, {element2}, {element3}");
            }

            ClearSelectedElements();
        }

        /// <summary>
        ///     1단계 타워를 생성하는 함수
        /// </summary>
        /// <param name="elementType">생성할 타워의 원소 타입</param>
        private void CreateLevel1Tower(ElementType elementType)
        {
            if (_elementTowerMap.TryGetValue(elementType, out var towerPrefab))
            {
                if (towerPrefab)
                {
                    var newTower = Instantiate(towerPrefab, spawnPosition, Quaternion.identity);
                    if (towerParent) newTower.transform.SetParent(towerParent);
                    
                    newTower.name = $"{elementType}_Tower_Level1";

                    OnTowerCreated(newTower, elementType);
                    
                    Debug.Log($"{elementType} 타입의 1단계 타워가 {spawnPosition}에 생성되었습니다!");
                }
                else
                {
                    Debug.LogError($"{elementType} 원소에 대한 타워 프리팹이 설정되지 않았습니다!");
                }
            }
            else
            {
                Debug.LogError($"{elementType} 원소가 타워 매핑에 존재하지 않습니다!");
            }
        }

        /// <summary>
        ///     타워가 생성되었을 때 호출되는 함수
        ///     추가적인 로직을 여기에 구현할 수 있습니다
        /// </summary>
        /// <param name="createdTower">생성된 타워 게임오브젝트</param>
        /// <param name="elementType">타워의 원소 타입</param>
        private void OnTowerCreated(GameObject createdTower, ElementType elementType)
        {
            // 사운드 재생, 이펙트 생성, UI 업데이트 등
            
            var towerComponent = createdTower.GetComponent<Tower>();
            if (towerComponent != null)
                // etc
                Debug.Log($"타워 컴포넌트 설정 완료: {towerComponent.GetTowerSetting().name}");
        }

        /// <summary>
        ///     선택된 원소들을 초기화하는 함수
        ///     조합 후나 수동으로 초기화할 때 사용됩니다
        /// </summary>
        public void ClearSelectedElements()
        {
            _selectedElements.Clear();
            Debug.Log("선택된 원소들이 초기화되었습니다.");
        }

        /// <summary>
        ///     타워 생성 위치를 설정하는 함수
        /// </summary>
        /// <param name="position">새로운 생성 위치</param>
        public void SetSpawnPosition(Vector3 position)
        {
            spawnPosition = position;
            Debug.Log($"타워 생성 위치가 {position}로 설정되었습니다.");
        }

        /// <summary>
        ///     현재 선택된 원소들을 반환하는 함수
        ///     UI에서 현재 상태를 표시할 때 사용할 수 있습니다
        /// </summary>
        /// <returns>선택된 원소들의 리스트 복사본</returns>
        public List<ElementType> GetSelectedElements()
        {
            return new List<ElementType>(_selectedElements);
        }

        /// <summary>
        ///     테스트용 함수
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectElement(ElementType.Fire);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectElement(ElementType.Water);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectElement(ElementType.Lightning);
            if (Input.GetKeyDown(KeyCode.R)) ClearSelectedElements(); // R키로 초기화
        }
    }
}