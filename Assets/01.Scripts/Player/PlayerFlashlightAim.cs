using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerFlashlightAim : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _aimPivot;

    private void Awake()
    {
        if (_mainCamera != null)
        {
            return;
        }

        // POC에서는 Main Camera 참조를 자동으로 잡되, 가능하면 Inspector 연결을 권장한다.
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_mainCamera == null || _aimPivot == null)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        // New Input System 방식으로 마우스 화면 좌표를 읽는다.
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        // 마우스 화면 좌표를 월드 좌표로 변환한다.
        Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = _aimPivot.position.z;

        // 플래시라이트 기준에서 마우스 방향을 계산한다.
        Vector2 aimDirection = mouseWorldPosition - _aimPivot.position;

        if (aimDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        // 2D에서 오른쪽 방향을 기준으로 마우스 방향 회전값을 계산한다.
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // 플래시라이트 Pivot을 마우스 방향으로 회전한다.
        _aimPivot.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
    }
}