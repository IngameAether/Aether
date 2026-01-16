using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 클릭 관리 매니저
/// </summary>
public class ClickManager : MonoBehaviour
{
    public static ClickManager Instance { get; private set; }

    //[Header("Double Click Settings")]
    //[SerializeField] private float doubleClickTime = 0.3f;

    public event Action<Tower> OnTowerDoubleClicked;
    public event Action OnClickOutside; // 빈 공간 클릭 (UI숨김용)

    private readonly RaycastHit2D[] _hits = new RaycastHit2D[5];
    private Camera _camera;
    private float _lastClickTime = 0f;
    private Tower _lastClickedTower = null;
    private Coroutine _singleClickRoutine;

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
    /// 타워 클릭 처리 (단일클릭은 지연, 더블클릭 시 단일클릭 예약 취소)
    /// </summary>
    private void HandleTowerClick(Tower tower, TowerSelectable towerSelectable)
    {
        if (tower == null || towerSelectable == null)
        {
            return;
        }

        towerSelectable.OnClick();

        //float currentTime = Time.time;

        //// 같은 타워로 더블클릭인지 판정
        //if (_lastClickedTower == tower && (currentTime - _lastClickTime) <= doubleClickTime)
        //{
        //    // 이미 예약된 단일클릭 취소 -> 더블클릭만 실행
        //    if (_singleClickRoutine != null)
        //    {
        //        StopCoroutine(_singleClickRoutine);
        //        _singleClickRoutine = null;
        //    }

        //    OnTowerDoubleClicked?.Invoke(tower);
        //    ResetDoubleClickState();
        //    Debug.Log($"타워 더블클릭: {tower.name} - UI 표시");
        //}
        //else
        //{
        //    // 기존 예약 단일클릭이 있다면 취소(타워 전환 등)
        //    if (_singleClickRoutine != null)
        //    {
        //        StopCoroutine(_singleClickRoutine);
        //        _singleClickRoutine = null;
        //    }

        //    // 더블클릭 대기 상태 업데이트
        //    _lastClickedTower = tower;
        //    _lastClickTime = currentTime;

        //    // 단일클릭은 doubleClickTime 뒤로 지연 예약
        //    _singleClickRoutine = StartCoroutine(DelayedSingleClick(towerSelectable));
        //    Debug.Log($"타워 단일클릭 대기: {tower.name}");
        //}
    }

    /// <summary>
    /// 단일클릭은 doubleClickTime만큼 대기 후 실행(그 사이 더블클릭 오면 취소됨)
    /// </summary>
    //private IEnumerator DelayedSingleClick(TowerSelectable towerSelectable)
    //{
    //    // 대기 시간은 doubleClickTime과 동일하게 유지
    //    yield return new WaitForSeconds(doubleClickTime); // WaitForSeconds는 지정 시간 경과 후 다음 프레임에 재개됨

    //    // 더블클릭이 오지 않았으면 단일클릭 수행
    //    towerSelectable.OnClick();

    //    // 상태 정리
    //    _singleClickRoutine = null;
    //    ResetDoubleClickState();
    //}
    /// <summary>
    /// 더블클릭 상태 리셋
    /// </summary>
    private void ResetDoubleClickState()
    {
        _lastClickedTower = null;
        _lastClickTime = 0f;
    }
}
