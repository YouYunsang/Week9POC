using UnityEngine;

public class HideCursor : MonoBehaviour
{
    [SerializeField] private bool _hideCursorOnStart = true;

    private void Start()
    {
        if (!_hideCursorOnStart)
        {
            return;
        }

        // 마우스 조준은 유지해야 하므로 커서는 숨기되 잠그지 않는다.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }
}
