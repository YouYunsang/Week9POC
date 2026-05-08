using System;
using UnityEngine;

public sealed class OxygenWarningViewModel : MonoBehaviour
{
    private const float LOW_THRESHOLD = 0.5f;
    private const float WARNING_THRESHOLD = 0.3f;
    private const float CRITICAL_THRESHOLD = 0.15f;

    [SerializeField] private PlayerOxygen _playerOxygen;

    [Header("Vignette Alpha")]
    [SerializeField] private float _lowVignetteAlpha = 0.1f;
    [SerializeField] private float _warningVignetteAlpha = 0.15f;
    [SerializeField] private float _criticalVignetteAlpha = 0.2f;
    [SerializeField] private float _depletedVignetteAlpha = 0.3f;

    [Header("Overlay Alpha")]
    [SerializeField] private float _lowOverlayAlpha = 0.05f;
    [SerializeField] private float _warningOverlayAlpha = 0.12f;
    [SerializeField] private float _criticalOverlayAlpha = 0.22f;
    [SerializeField] private float _depletedOverlayAlpha = 0.35f;

    public event Action<OxygenWarningViewState> ViewStateChanged;

    private void OnEnable()
    {
        if (_playerOxygen == null)
        {
            return;
        }

        // 산소 변경 이벤트를 구독한다.
        _playerOxygen.OxygenChanged += HandleOxygenChanged;

        // 활성화 시 현재 산소 상태를 즉시 반영한다.
        PublishViewState(_playerOxygen.CurrentOxygen, _playerOxygen.MaxOxygen);
    }

    private void OnDisable()
    {
        if (_playerOxygen == null)
        {
            return;
        }

        // 비활성화 시 이벤트 구독을 해제한다.
        _playerOxygen.OxygenChanged -= HandleOxygenChanged;
    }

    private void HandleOxygenChanged(float currentOxygen, float maxOxygen)
    {
        // 산소 변경값을 경고 표시 상태로 변환한다.
        PublishViewState(currentOxygen, maxOxygen);
    }

    private void PublishViewState(float currentOxygen, float maxOxygen)
    {
        float oxygenRatio = maxOxygen <= 0.0f
            ? 0.0f
            : Mathf.Clamp01(currentOxygen / maxOxygen);

        OxygenWarningStage stage = GetWarningStage(oxygenRatio);
        float intensity = GetIntensity(stage, oxygenRatio);

        OxygenWarningViewState viewState = new OxygenWarningViewState(
            stage,
            intensity,
            GetVignetteAlpha(stage),
            GetOverlayAlpha(stage),
            ShouldShowWarningText(stage),
            GetWarningText(stage));

        ViewStateChanged?.Invoke(viewState);
    }

    private OxygenWarningStage GetWarningStage(float oxygenRatio)
    {
        if (oxygenRatio <= 0.0f)
        {
            return OxygenWarningStage.Depleted;
        }

        if (oxygenRatio <= CRITICAL_THRESHOLD)
        {
            return OxygenWarningStage.Critical;
        }

        if (oxygenRatio <= WARNING_THRESHOLD)
        {
            return OxygenWarningStage.Warning;
        }

        if (oxygenRatio <= LOW_THRESHOLD)
        {
            return OxygenWarningStage.Low;
        }

        return OxygenWarningStage.Normal;
    }

    private float GetIntensity(OxygenWarningStage stage, float oxygenRatio)
    {
        switch (stage)
        {
            case OxygenWarningStage.Low:
                return Mathf.InverseLerp(LOW_THRESHOLD, WARNING_THRESHOLD, oxygenRatio);

            case OxygenWarningStage.Warning:
                return Mathf.InverseLerp(WARNING_THRESHOLD, CRITICAL_THRESHOLD, oxygenRatio);

            case OxygenWarningStage.Critical:
                return Mathf.InverseLerp(CRITICAL_THRESHOLD, 0.0f, oxygenRatio);

            case OxygenWarningStage.Depleted:
                return 1.0f;

            default:
                return 0.0f;
        }
    }

    private float GetVignetteAlpha(OxygenWarningStage stage)
    {
        switch (stage)
        {
            case OxygenWarningStage.Low:
                return _lowVignetteAlpha;

            case OxygenWarningStage.Warning:
                return _warningVignetteAlpha;

            case OxygenWarningStage.Critical:
                return _criticalVignetteAlpha;

            case OxygenWarningStage.Depleted:
                return _depletedVignetteAlpha;

            default:
                return 0.0f;
        }
    }

    private float GetOverlayAlpha(OxygenWarningStage stage)
    {
        switch (stage)
        {
            case OxygenWarningStage.Low:
                return _lowOverlayAlpha;

            case OxygenWarningStage.Warning:
                return _warningOverlayAlpha;

            case OxygenWarningStage.Critical:
                return _criticalOverlayAlpha;

            case OxygenWarningStage.Depleted:
                return _depletedOverlayAlpha;

            default:
                return 0.0f;
        }
    }

    private bool ShouldShowWarningText(OxygenWarningStage stage)
    {
        return stage == OxygenWarningStage.Critical ||
               stage == OxygenWarningStage.Depleted;
    }

    private string GetWarningText(OxygenWarningStage stage)
    {
        switch (stage)
        {
            case OxygenWarningStage.Critical:
                return "OXYGEN DEFICIENCY!";

            case OxygenWarningStage.Depleted:
                return "OXYGEN DEPLETION!";

            default:
                return string.Empty;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 알파 값들은 UI 표시 범위를 넘지 않게 제한한다.
        _lowVignetteAlpha = Mathf.Clamp01(_lowVignetteAlpha);
        _warningVignetteAlpha = Mathf.Clamp01(_warningVignetteAlpha);
        _criticalVignetteAlpha = Mathf.Clamp01(_criticalVignetteAlpha);
        _depletedVignetteAlpha = Mathf.Clamp01(_depletedVignetteAlpha);

        _lowOverlayAlpha = Mathf.Clamp01(_lowOverlayAlpha);
        _warningOverlayAlpha = Mathf.Clamp01(_warningOverlayAlpha);
        _criticalOverlayAlpha = Mathf.Clamp01(_criticalOverlayAlpha);
        _depletedOverlayAlpha = Mathf.Clamp01(_depletedOverlayAlpha);
    }
#endif
}