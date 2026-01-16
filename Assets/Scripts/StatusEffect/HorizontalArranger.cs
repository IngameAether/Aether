using System.Collections.Generic;
using UnityEngine;

public class HorizontalArranger : MonoBehaviour
{
    private float spacing = 0.5f; // 오브젝트 사이의 간격

    // 자식 오브젝트가 활성화될 때 호출될 함수
    public void ArrangeChildren()
    {
        // 1. 활성화된 자식들만 리스트에 담기
        List<Transform> activeChildren = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                activeChildren.Add(child);
            }
        }

        int count = activeChildren.Count;
        if (count == 0) return;

        // 2. 전체 너비 계산 (간격 * (개수 - 1))
        float totalWidth = (count - 1) * spacing;

        // 3. 시작 지점 (가장 왼쪽 오브젝트의 위치)
        float startX = -totalWidth / 2f;

        // 4. 순차적으로 배치
        for (int i = 0; i < count; i++)
        {
            float xPos = startX + (i * spacing);
            activeChildren[i].localPosition = new Vector3(xPos, 0, 0);
        }
    }
}
