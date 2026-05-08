using UnityEngine;
using UnityEngine.Rendering.Universal;

public sealed class PlayerFlashlight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light2D _flashlight;
    [SerializeField] private Collider2D _detectionCollider;

    [Header("Initial State")]
    [SerializeField] private bool _isOnAtStart = false;

    public bool IsOn { get; private set; }
    public Vector2 OriginPosition => transform.position;

    private void Awake()
    {
        // 시작 상태에 맞춰 플래시라이트를 초기화한다.
        SetFlashlightEnabled(_isOnAtStart);
    }

    public void Toggle()
    {
        // F 입력으로 플래시라이트 상태를 반전한다.
        SetFlashlightEnabled(!IsOn);
    }

    public void SetFlashlightEnabled(bool isEnabled)
    {
        IsOn = isEnabled;

        if (_flashlight != null)
        {
            // 실제 Light2D 시각 표현을 켜거나 끈다.
            _flashlight.enabled = IsOn;
        }

        if (_detectionCollider != null)
        {
            // 적 빛 감지용 Cone Trigger를 켜거나 끈다.
            _detectionCollider.enabled = IsOn;
        }

        Debug.Log(IsOn ? "플래시라이트 ON" : "플래시라이트 OFF");
    }
}