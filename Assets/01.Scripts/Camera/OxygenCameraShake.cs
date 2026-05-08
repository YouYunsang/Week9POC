using Unity.Cinemachine;
using UnityEngine;

public sealed class OxygenCameraShake : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OxygenWarningViewModel _viewModel;
    [SerializeField] private CinemachineBasicMultiChannelPerlin _noise;

    [Header("Low")]
    [SerializeField] private float _lowAmplitude = 0.0f;
    [SerializeField] private float _lowFrequency = 0.0f;

    [Header("Warning")]
    [SerializeField] private float _warningAmplitude = 0.15f;
    [SerializeField] private float _warningFrequency = 0.6f;

    [Header("Critical")]
    [SerializeField] private float _criticalAmplitude = 0.35f;
    [SerializeField] private float _criticalFrequency = 1.2f;

    [Header("Depleted")]
    [SerializeField] private float _depletedAmplitude = 0.5f;
    [SerializeField] private float _depletedFrequency = 1.8f;

    [Header("Smooth")]
    [SerializeField] private float _shakeSmoothSpeed = 8.0f;

    private float _targetAmplitude;
    private float _targetFrequency;

    private void OnEnable()
    {
        if (_viewModel == null)
        {
            return;
        }

        // 산소 경고 상태 변경 이벤트를 구독한다.
        _viewModel.ViewStateChanged += HandleViewStateChanged;
    }

    private void OnDisable()
    {
        if (_viewModel == null)
        {
            return;
        }

        // 비활성화 시 이벤트 구독을 해제한다.
        _viewModel.ViewStateChanged -= HandleViewStateChanged;
    }

    private void Update()
    {
        if (_noise == null)
        {
            return;
        }

        // 흔들림 수치가 갑자기 튀지 않도록 부드럽게 보간한다.
        _noise.AmplitudeGain = Mathf.Lerp(
            _noise.AmplitudeGain,
            _targetAmplitude,
            _shakeSmoothSpeed * Time.deltaTime);

        _noise.FrequencyGain = Mathf.Lerp(
            _noise.FrequencyGain,
            _targetFrequency,
            _shakeSmoothSpeed * Time.deltaTime);
    }

    private void HandleViewStateChanged(OxygenWarningViewState viewState)
    {
        switch (viewState.Stage)
        {
            case OxygenWarningStage.Low:
                SetTargetShake(_lowAmplitude, _lowFrequency);
                break;

            case OxygenWarningStage.Warning:
                SetTargetShake(_warningAmplitude, _warningFrequency);
                break;

            case OxygenWarningStage.Critical:
                SetTargetShake(_criticalAmplitude, _criticalFrequency);
                break;

            case OxygenWarningStage.Depleted:
                SetTargetShake(_depletedAmplitude, _depletedFrequency);
                break;

            default:
                SetTargetShake(0.0f, 0.0f);
                break;
        }
    }

    private void SetTargetShake(float amplitude, float frequency)
    {
        _targetAmplitude = Mathf.Max(0.0f, amplitude);
        _targetFrequency = Mathf.Max(0.0f, frequency);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 흔들림 값은 음수가 되지 않게 제한한다.
        _lowAmplitude = Mathf.Max(0.0f, _lowAmplitude);
        _lowFrequency = Mathf.Max(0.0f, _lowFrequency);

        _warningAmplitude = Mathf.Max(0.0f, _warningAmplitude);
        _warningFrequency = Mathf.Max(0.0f, _warningFrequency);

        _criticalAmplitude = Mathf.Max(0.0f, _criticalAmplitude);
        _criticalFrequency = Mathf.Max(0.0f, _criticalFrequency);

        _depletedAmplitude = Mathf.Max(0.0f, _depletedAmplitude);
        _depletedFrequency = Mathf.Max(0.0f, _depletedFrequency);

        _shakeSmoothSpeed = Mathf.Max(0.01f, _shakeSmoothSpeed);
    }
#endif
}