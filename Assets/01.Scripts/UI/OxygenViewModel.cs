using System;
using UnityEngine;

public sealed class OxygenViewModel : MonoBehaviour
{
    [SerializeField] private PlayerOxygen _playerOxygen;

    public event Action<OxygenViewState> ViewStateChanged;

    private void OnEnable()
    {
        if (_playerOxygen == null)
        {
            return;
        }

        // Model의 산소 변경 이벤트를 구독한다.
        _playerOxygen.OxygenChanged += HandleOxygenChanged;

        // 활성화 시 현재 Model 값을 즉시 View에 반영한다.
        PublishViewState(_playerOxygen.CurrentOxygen, _playerOxygen.MaxOxygen);
    }

    private void OnDisable()
    {
        if (_playerOxygen == null)
        {
            return;
        }

        // 비활성화 시 구독을 해제한다.
        _playerOxygen.OxygenChanged -= HandleOxygenChanged;
    }

    private void HandleOxygenChanged(float currentOxygen, float maxOxygen)
    {
        // Model 변경값을 View 표시 상태로 변환한다.
        PublishViewState(currentOxygen, maxOxygen);
    }

    private void PublishViewState(float currentOxygen, float maxOxygen)
    {
        float ratio = maxOxygen <= 0.0f ? 0.0f : currentOxygen / maxOxygen;
        string amountText = $"{Mathf.CeilToInt(currentOxygen)} / {Mathf.CeilToInt(maxOxygen)}";

        OxygenViewState viewState = new OxygenViewState(
            ratio,
            amountText,
            currentOxygen <= 0.0f);

        // View가 화면 표시만 할 수 있도록 가공된 상태를 전달한다.
        ViewStateChanged?.Invoke(viewState);
    }
}

public readonly struct OxygenViewState
{
    public OxygenViewState(float ratio, string amountText, bool isDepleted)
    {
        Ratio = ratio;
        AmountText = amountText;
        IsDepleted = isDepleted;
    }

    public float Ratio { get; }
    public string AmountText { get; }
    public bool IsDepleted { get; }
}