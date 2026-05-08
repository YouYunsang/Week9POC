using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class OxygenWarningView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OxygenWarningViewModel _viewModel;
    [SerializeField] private Image _vignetteImage;
    [SerializeField] private Image _colorOverlayImage;
    [SerializeField] private TMP_Text _warningText;

    [Header("Pulse")]
    [SerializeField] private float _criticalPulseSpeed = 4.0f;
    [SerializeField] private float _criticalPulseStrength = 0.2f;

    private OxygenWarningViewState _currentViewState;

    private void OnEnable()
    {
        if (_viewModel == null)
        {
            return;
        }

        // ViewModel의 표시 상태 변경 이벤트를 구독한다.
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
        if (_currentViewState.Stage != OxygenWarningStage.Critical &&
            _currentViewState.Stage != OxygenWarningStage.Depleted)
        {
            return;
        }

        // 사망 직전에는 맥박처럼 알파를 출렁이게 만든다.
        ApplyPulseEffect();
    }

    private void HandleViewStateChanged(OxygenWarningViewState viewState)
    {
        _currentViewState = viewState;

        // View는 ViewModel이 준 표시 상태를 화면에 반영만 한다.
        ApplyVignette(viewState.VignetteAlpha);
        ApplyOverlay(viewState.OverlayAlpha);
        ApplyWarningText(viewState.ShouldShowWarningText, viewState.WarningText);
    }

    private void ApplyVignette(float alpha)
    {
        if (_vignetteImage == null)
        {
            return;
        }

        Color color = _vignetteImage.color;
        color.a = alpha;
        _vignetteImage.color = color;
    }

    private void ApplyOverlay(float alpha)
    {
        if (_colorOverlayImage == null)
        {
            return;
        }

        Color color = _colorOverlayImage.color;
        color.a = alpha;
        _colorOverlayImage.color = color;
    }

    private void ApplyWarningText(bool shouldShow, string text)
    {
        if (_warningText == null)
        {
            return;
        }

        _warningText.gameObject.SetActive(shouldShow);
        _warningText.text = text;
    }

    private void ApplyPulseEffect()
    {
        float pulse = (Mathf.Sin(Time.time * _criticalPulseSpeed) + 1.0f) * 0.5f;
        float pulseAlphaOffset = pulse * _criticalPulseStrength;

        if (_vignetteImage != null)
        {
            Color vignetteColor = _vignetteImage.color;
            vignetteColor.a = Mathf.Clamp01(_currentViewState.VignetteAlpha + pulseAlphaOffset);
            _vignetteImage.color = vignetteColor;
        }

        if (_warningText != null && _warningText.gameObject.activeSelf)
        {
            Color textColor = _warningText.color;
            textColor.a = Mathf.Clamp01(0.65f + pulse * 0.35f);
            _warningText.color = textColor;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 맥박 속도와 강도는 음수가 되지 않게 제한한다.
        _criticalPulseSpeed = Mathf.Max(0.0f, _criticalPulseSpeed);
        _criticalPulseStrength = Mathf.Clamp01(_criticalPulseStrength);
    }
#endif
}