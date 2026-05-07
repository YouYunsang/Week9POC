using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CameraInputReader))]
public sealed class PlayerCameraZoom : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private CameraZoomData _zoomData;

    private CameraInputReader _inputReader;

    private float _targetOrthographicSize;
    private float _currentOrthographicSize;
    private float _zoomVelocity;

    private void Awake()
    {
        // 같은 GameObject에 붙은 입력 리더를 Awake에서 캐싱한다.
        _inputReader = GetComponent<CameraInputReader>();

        if (_zoomData == null)
        {
            return;
        }

        // 시작 줌 값을 ScriptableObject 기본값으로 초기화한다.
        _targetOrthographicSize = _zoomData.DefaultOrthographicSize;
        _currentOrthographicSize = _zoomData.DefaultOrthographicSize;

        ApplyOrthographicSize(_currentOrthographicSize);
    }

    private void OnEnable()
    {
        // 줌 입력 이벤트를 구독한다.
        _inputReader.ZoomInputChanged += HandleZoomInputChanged;
    }

    private void OnDisable()
    {
        // 비활성화 시 이벤트 구독을 해제한다.
        _inputReader.ZoomInputChanged -= HandleZoomInputChanged;
    }

    private void LateUpdate()
    {
        if (_cinemachineCamera == null || _zoomData == null)
        {
            return;
        }

        // 목표 줌 값까지 부드럽게 보간한다.
        _currentOrthographicSize = Mathf.SmoothDamp(
            _currentOrthographicSize,
            _targetOrthographicSize,
            ref _zoomVelocity,
            _zoomData.ZoomSmoothTime);

        ApplyOrthographicSize(_currentOrthographicSize);
    }

    private void HandleZoomInputChanged(float scrollY)
    {
        if (_zoomData == null)
        {
            return;
        }

        if (Mathf.Approximately(scrollY, 0.0f))
        {
            return;
        }

        // 마우스 휠 위쪽 입력은 줌인, 아래쪽 입력은 줌아웃으로 처리한다.
        float zoomDelta = -Mathf.Sign(scrollY) * _zoomData.ZoomSensitivity;

        _targetOrthographicSize = Mathf.Clamp(
            _targetOrthographicSize + zoomDelta,
            _zoomData.MinOrthographicSize,
            _zoomData.MaxOrthographicSize);
    }

    private void ApplyOrthographicSize(float orthographicSize)
    {
        if (_cinemachineCamera == null)
        {
            return;
        }

        // CinemachineCamera의 Lens는 구조체이므로 복사 후 다시 대입한다.
        LensSettings lens = _cinemachineCamera.Lens;
        lens.OrthographicSize = orthographicSize;
        _cinemachineCamera.Lens = lens;
    }
}