using UnityEngine;

[CreateAssetMenu(
    fileName = "CameraZoomData",
    menuName = "ScriptableObjects/Camera/Camera Zoom Data")]
public sealed class CameraZoomData : ScriptableObject
{
    [Header("Orthographic Zoom")]
    [SerializeField] private float _defaultOrthographicSize = 6.0f;
    [SerializeField] private float _minOrthographicSize = 3.5f;
    [SerializeField] private float _maxOrthographicSize = 8.5f;

    [Header("Zoom Control")]
    [SerializeField] private float _zoomSensitivity = 0.4f;
    [SerializeField] private float _zoomSmoothTime = 0.12f;

    public float DefaultOrthographicSize => _defaultOrthographicSize;
    public float MinOrthographicSize => _minOrthographicSize;
    public float MaxOrthographicSize => _maxOrthographicSize;
    public float ZoomSensitivity => _zoomSensitivity;
    public float ZoomSmoothTime => _zoomSmoothTime;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 카메라 크기는 0 이하가 되면 화면 렌더링이 깨지므로 최소값을 보장한다.
        _minOrthographicSize = Mathf.Max(0.1f, _minOrthographicSize);
        _maxOrthographicSize = Mathf.Max(_minOrthographicSize, _maxOrthographicSize);

        // 기본 줌 값은 최소/최대 범위 안에 있도록 제한한다.
        _defaultOrthographicSize = Mathf.Clamp(
            _defaultOrthographicSize,
            _minOrthographicSize,
            _maxOrthographicSize);

        // 감도와 보간 시간은 음수가 되지 않게 제한한다.
        _zoomSensitivity = Mathf.Max(0.01f, _zoomSensitivity);
        _zoomSmoothTime = Mathf.Max(0.01f, _zoomSmoothTime);
    }
#endif
}