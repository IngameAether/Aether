using UnityEngine;

public class test : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) // L 키 누르면 빛 원소 +1
        {
            ResourceManager.Instance.AddElement(ReinforceType.Light, 1);
        }

        if (Input.GetKeyDown(KeyCode.D)) // D 키 누르면 어둠 원소 +1
        {
            ResourceManager.Instance.AddElement(ReinforceType.Dark, 1);
        }
    }
}
