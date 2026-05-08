using System;
using UnityEngine;

[RequireComponent(typeof(PlayerCondition))]
public sealed class PlayerOxygen : MonoBehaviour
{
    [SerializeField] private PlayerOxygenData _oxygenData;
    [SerializeField] private bool _consumeOxygenOnStart = true;

    private PlayerCondition _condition;

    private float _currentOxygen;
    private float _extraConsumePerSecond;
    private Vector2 _moveInput;
    private bool _isSprinting;
    private bool _isConsumingOxygen;

    public event Action<float, float> OxygenChanged;
    public event Action OxygenDepleted;

    public float CurrentOxygen => _currentOxygen;
    public float MaxOxygen => _oxygenData != null ? _oxygenData.MaxOxygen : 0.0f;
    public float OxygenRatio => MaxOxygen <= 0.0f ? 0.0f : _currentOxygen / MaxOxygen;
    public float CrowbarUseConsumePerSecond => _oxygenData != null ? _oxygenData.CrowbarUseConsumePerSecond : 0.0f;
    public bool IsDepleted => _currentOxygen <= 0.0f;

    private void Awake()
    {
        // 같은 GameObject에 붙은 상태 컴포넌트를 캐싱한다.
        _condition = GetComponent<PlayerCondition>();

        // 시작 시 산소를 최대치로 초기화한다.
        InitializeOxygen();
    }

    private void OnEnable()
    {
        // 탐색 종료 시 산소 소모를 멈춘다.
        GameEventBus.RunEnded += HandleRunEnded;
    }

    private void OnDisable()
    {
        GameEventBus.RunEnded -= HandleRunEnded;
    }

    private void HandleRunEnded(GameResultType resultType)
    {
        // 복귀 성공 또는 익사 실패 후에는 산소 소모를 더 진행하지 않는다.
        StopConsumingOxygen();
        ClearExtraConsumePerSecond();
    }

    private void Update()
    {
        if (!_isConsumingOxygen)
        {
            return;
        }

        if (_condition.IsDead)
        {
            return;
        }

        // 현재 이동 상태에 맞는 초당 산소 소모량을 계산한다.
        float consumeAmount = CalculateConsumePerSecond() * Time.deltaTime;

        // 산소를 실제로 차감한다.
        ConsumeOxygen(consumeAmount);
    }

    public void SetMovementState(Vector2 moveInput, bool isSprinting)
    {
        // Controller에서 받은 이동 입력 상태를 산소 소모 계산에 사용한다.
        _moveInput = moveInput;
        _isSprinting = isSprinting;
    }

    public void SetExtraConsumePerSecond(float consumePerSecond)
    {
        // 쇠지레 사용 같은 추가 행동 산소 소모를 설정한다.
        _extraConsumePerSecond = Mathf.Max(0.0f, consumePerSecond);
    }

    public void ClearExtraConsumePerSecond()
    {
        _extraConsumePerSecond = 0f;
    }

    public void StartConsumingOxygen()
    {
        // 외부에서 산소 소모를 시작할 수 있게 한다.
        _isConsumingOxygen = true;
    }

    public void StopConsumingOxygen()
    {
        // 수면 위, 컷신, 결과 화면 등에서 산소 소모를 멈출 수 있게 한다.
        _isConsumingOxygen = false;
    }

    public void ResetOxygen()
    {
        // 재시작이나 디버그용으로 산소를 최대치로 복구한다.
        InitializeOxygen();
    }

    public void ConsumeDefaultInstantOxygen()
    {
        if (_oxygenData == null)
        {
            return;
        }

        // 쇠지레, 발각, 밀치기 같은 행동에서 기본 즉시 소모량을 사용할 수 있게 한다.
        ConsumeOxygen(_oxygenData.DefaultInstantConsumeAmount);
    }

    public void ConsumeOxygen(float amount)
    {
        if (_condition.IsDead)
        {
            return;
        }

        if (_oxygenData == null)
        {
            return;
        }

        if (amount <= 0.0f)
        {
            return;
        }

        // 산소를 0과 최대값 사이로 제한한다.
        _currentOxygen = Mathf.Clamp(_currentOxygen - amount, 0.0f, _oxygenData.MaxOxygen);

        // Model의 산소 변경을 외부에 알린다.
        OxygenChanged?.Invoke(_currentOxygen, _oxygenData.MaxOxygen);

        if (_currentOxygen > 0.0f)
        {
            return;
        }

        // 산소가 0이 되면 고갈 이벤트를 한 번 발생시킨다.
        OxygenDepleted?.Invoke();
    }

    private void InitializeOxygen()
    {
        if (_oxygenData == null)
        {
            _currentOxygen = 0.0f;
            _isConsumingOxygen = false;
            _extraConsumePerSecond = 0.0f;
            return;
        }

        _currentOxygen = _oxygenData.MaxOxygen;
        _isConsumingOxygen = _consumeOxygenOnStart;
        _extraConsumePerSecond = 0f;

        // 초기 산소값도 UI에 반영될 수 있게 알린다.
        OxygenChanged?.Invoke(_currentOxygen, _oxygenData.MaxOxygen);
    }

    private float CalculateConsumePerSecond()
    {
        if (_oxygenData == null)
        {
            return 0.0f;
        }

        float baseConsumePerSecond;

        if (_isSprinting)
        {
            // 빠른 헤엄은 가장 높은 산소 소모를 사용한다.
            baseConsumePerSecond = _oxygenData.SprintConsumePerSecond;
        }
        else if(_moveInput.sqrMagnitude > 0.01f)
        {
            baseConsumePerSecond = _oxygenData.MoveConsumePerSecond;
        }
        else
        {
            baseConsumePerSecond = _oxygenData.IdleConsumePerSecond;
        }

        return baseConsumePerSecond + _extraConsumePerSecond;
    }
}