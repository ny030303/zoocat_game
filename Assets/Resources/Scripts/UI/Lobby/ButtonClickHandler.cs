using UnityEngine;
using UnityEngine.UI;

public class ButtonClickHandler : MonoBehaviour
{
    // 비활성화할 대상
    public GameObject targetObject;

    // 버튼 클릭 시 호출될 메서드
    public void HideObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false); // GameObject를 비활성화
        }
    }
}
