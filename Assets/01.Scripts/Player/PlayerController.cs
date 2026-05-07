using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCondition))]
[RequireComponent(typeof(PlayerOxygen))]
[RequireComponent(typeof(PlayerInteractionDetector))]
public sealed class PlayerController : MonoBehaviour
{
    private PlayerInputReader _inputReader;
    private PlayerMovement _movement;
    private PlayerCondition _condition;
    private PlayerOxygen _oxygen;
    private PlayerInteractionDetector _interactionDetector;

    private Vector2 _moveInput;
    private bool _isSprinting;

    public bool IsSprinting => _isSprinting;
    public bool CanMove => _condition != null && _condition.CanMove;

    private void Reset()
    {
        // 같은 GameObject 내부 컴포넌트를 Awake에서 캐싱한다.
        _inputReader = GetComponent<PlayerInputReader>();
        _movement = GetComponent<PlayerMovement>();
        _condition = GetComponent<PlayerCondition>();
        _oxygen = GetComponent<PlayerOxygen>();
        _interactionDetector = GetComponent<PlayerInteractionDetector>();
    }

    private void Awake()
    {
        // 같은 GameObject 내부 컴포넌트를 Awake에서 캐싱한다.
        _inputReader = GetComponent<PlayerInputReader>();
        _movement = GetComponent<PlayerMovement>();
        _condition = GetComponent<PlayerCondition>();
        _oxygen = GetComponent<PlayerOxygen>();
        _interactionDetector = GetComponent<PlayerInteractionDetector>();
    }

    private void OnEnable()
    {
        // 입력 이벤트를 구독한다.
        _inputReader.MoveInputChanged += HandleMoveInputChanged;
        _inputReader.SprintInputChanged += HandleSprintInputChanged;
        _inputReader.InteractInputStarted += HandleInteractInputStarted;

        // 산소 고갈 이벤트를 구독한다.
        _oxygen.OxygenDepleted += HandleOxygenDepleted;
    }

    private void OnDisable()
    {
        // 비활성화 시 이벤트 구독을 해제해 중복 호출을 방지한다.
        _inputReader.MoveInputChanged -= HandleMoveInputChanged;
        _inputReader.SprintInputChanged -= HandleSprintInputChanged;
        _inputReader.InteractInputStarted -= HandleInteractInputStarted;

        _oxygen.OxygenDepleted -= HandleOxygenDepleted;
    }

    private void Update()
    {
        // 현재 상태를 기준으로 실제 이동 입력을 결정한다.
        Vector2 finalMoveInput = _condition.CanMove ? _moveInput : Vector2.zero;
        bool finalSprintState = _condition.CanMove && _isSprinting;

        // 이동 컴포넌트에 최종 입력을 전달한다.
        _movement.SetMoveInput(finalMoveInput);
        _movement.SetSprintState(finalSprintState);

        // 산소 컴포넌트에는 현재 이동 상태를 전달한다.
        _oxygen.SetMovementState(finalMoveInput, finalSprintState);
    }

    public void SetMovementEnabled(bool isEnabled)
    {
        // 기존 외부 호출 호환을 위해 남겨두되, 실제 상태 관리는 PlayerCondition에 위임한다.
        _condition.SetMovementBlocked(!isEnabled);

        if (isEnabled)
        {
            return;
        }

        ClearMovementInput();
    }

    private void HandleMoveInputChanged(Vector2 moveInput)
    {
        if (!_condition.CanReceiveInput)
        {
            _moveInput = Vector2.zero;
            return;
        }

        // 입력값은 Controller가 보관하고, 실제 이동은 Update에서 일괄 전달한다.
        _moveInput = moveInput;
    }

    private void HandleSprintInputChanged(bool isSprinting)
    {
        if (!_condition.CanReceiveInput)
        {
            _isSprinting = false;
            return;
        }

        // 빠른 헤엄 상태를 저장한다.
        _isSprinting = isSprinting;
    }

    private void HandleInteractInputStarted()
    {
        if (!_condition.CanReceiveInput || !_condition.CanInteract)
        {
            return;
        }

        // 현재 감지 중인 상호작용 대상과 상호작용을 시도한다.
        _interactionDetector.TryInteract();
    }

    private void HandleOxygenDepleted()
    {
        // 산소 고갈은 플레이어 사망 상태로 전환된다.
        _condition.Die();

        // 사망 즉시 이동 입력을 제거한다.
        ClearMovementInput();
    }

    private void ClearMovementInput()
    {
        // 이동과 빠른 헤엄 입력을 초기화한다.
        _moveInput = Vector2.zero;
        _isSprinting = false;

        _movement.SetMoveInput(Vector2.zero);
        _movement.SetSprintState(false);
        _oxygen.SetMovementState(Vector2.zero, false);
    }
}