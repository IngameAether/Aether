using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 클릭 관리 매니저
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Double Click Settings")]
    [SerializeField] private float doubleClickTime = 0.3f;

    public event Action<Tower> OnTowerDoubleClicked;
    public event Action OnClickOutside; // 빈 공간 클릭 (UI숨김용)

    private Camera _camera;
    private float _lastClickTime = 0f;
    private Tower _lastClickedTower = null;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[5];

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _camera = Camera.main;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        HandleMouseClick();
    }

    /// <summary>
    /// 마우스 클릭 처리 (우선순위: 타워 > 타일)
    /// </summary>
    private void HandleMouseClick()
    {
        Vector2 worldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
        var size = Physics2D.RaycastNonAlloc(worldPoint, Vector2.zero, _hits);

        if (size == 0) return;

        // 타워가 막 생성된 직후면 UI 방지
        if (TileInteraction.isTowerJustCreated)
        {
            TileInteraction.isTowerJustCreated = false;
            return;
        }

        // 1순위: 타워 클릭 처리
        foreach (var hit in _hits)
        {
            if (!hit.collider) continue;

            var towerSelectable = hit.collider.GetComponent<TowerSelectable>();
            if (towerSelectable)
            {
                var tower = towerSelectable.GetComponent<Tower>();
                HandleTowerClick(tower, towerSelectable);
                return; // 이벤트 소비
            }
        }

        // 2순위: 타일 클릭 처리
        foreach (var hit in _hits)
        {
            if (hit.collider == null) continue;

            var tileInteraction = hit.collider.GetComponent<TileInteraction>();
            if (tileInteraction != null)
            {
                tileInteraction.OnClick();
                OnClickOutside?.Invoke(); // UI 숨김 이벤트 발생
                return; // 이벤트 소비
            }
        }

        // 빈 공간 클릭
        OnClickOutside?.Invoke();
        ResetDoubleClickState();
    }

    /// <summary>
    /// 타워 클릭 처리 (단일클릭 = 조합선택, 더블클릭 = UI표시)
    /// </summary>
    private void HandleTowerClick(Tower tower, TowerSelectable towerSelectable)
    {
        float currentTime = Time.time;

        // 같은 타워를 더블클릭 간격 내에 클릭했는지 확인
        if (_lastClickedTower == tower && (currentTime - _lastClickTime) <= doubleClickTime)
        {
            OnTowerDoubleClicked?.Invoke(tower);
            ResetDoubleClickState();
            Debug.Log($"타워 더블클릭: {tower.name} - UI 표시");
        }
        else
        {
            towerSelectable.OnClick();

            // 더블클릭 상태 업데이트
            _lastClickedTower = tower;
            _lastClickTime = currentTime;

            Debug.Log($"타워 단일클릭: {tower.name} - 조합 선택");
        }
    }

    /// <summary>
    /// 더블클릭 상태 리셋
    /// </summary>
    private void ResetDoubleClickState()
    {
        _lastClickedTower = null;
        _lastClickTime = 0f;
    }
}
