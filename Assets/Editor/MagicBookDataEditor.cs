using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MagicBookData))]
public class MagicBookDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Unity의 기본 인스펙터 그리기를 사용
        DrawDefaultInspector();

        // 만약 더 복잡한 커스텀 로직이 필요하다면 여기에 추가할 수 있습니다.
        // 예: 특정 EffectType을 선택하면 경고 메시지 표시 등
    }
}
