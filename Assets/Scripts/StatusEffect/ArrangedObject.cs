using UnityEngine;

public class ArrangedObject : MonoBehaviour
{
    private void OnEnable()
    {
        // 부모에게 정렬 로직 실행 요청
        var arranger = GetComponentInParent<HorizontalArranger>();
        if (arranger != null)
        {
            arranger.ArrangeChildren();
        }
    }
    private void OnDisable()
    {
        // 부모에게 정렬 로직 실행 요청
        var arranger = GetComponentInParent<HorizontalArranger>();
        if (arranger != null)
        {
            arranger.ArrangeChildren();
        }
    }
}
