using UnityEngine;

public class HorizontalArranger : MonoBehaviour
{
    public float spacing = 1.5f; // 오브젝트 사이의 간격

    // 자식 오브젝트가 활성화될 때 호출될 함수
    public void ArrangeChildren()
    {
        int activeCount = 0;

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                // 중앙정렬
                float totalWidth = (activeCount - 1) * spacing;
                child.localPosition = new Vector3((activeCount * spacing) - (totalWidth / 2f), 0, 0);
                activeCount++;
            }
        }
    }
}
