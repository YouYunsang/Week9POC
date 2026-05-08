using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class LockedDoor : MonoBehaviour, IHoldInteractable
{
    [Header("Requirement")]
    [SerializeField] private ToolType _requiredToolType = ToolType.Crowbar;

    [Header("Door")]
    [SerializeField] private float _openDuration = 3.0f;
    [SerializeField] private int _interactionPriority = 50;

    [Header("Oxygen")]
    [SerializeField] private bool _useCrowbarOxygenCost = true;

    [Header("Noise")]
    [SerializeField] private float _noiseInterval = 0.5f;
    [SerializeField] private float _noiseRadius = 6.0f;
    [SerializeField] private float _noiseIntensity = 1.0f;

    private float _currentProgress;
    private float _noiseTimer;
    private bool _isOpened;

    public int InteractionPriority => _interactionPriority;

    public bool CanInteract(InteractionContext context)
    {
        if (_isOpened)
        {
            return false;
        }

        if (context.PlayerCondition == null || context.PlayerCondition.IsDead)
        {
            return false;
        }

        if (context.PlayerInventory == null)
        {
            return false;
        }

        // 필요한 도구를 가지고 있어야 상호작용 가능하다.
        return context.PlayerInventory.HasTool(_requiredToolType);
    }

    public void Interact(InteractionContext context)
    {
        // Hold 대상은 PlayerHeldInteraction에서 BeginHold로 처리한다.
        if (!CanInteract(context))
        {
            Debug.Log("필요한 도구가 없어 문을 열 수 없습니다.");
            return;
        }

        Debug.Log("E를 누르고 유지하면 문을 열 수 있습니다.");
    }

    public string GetInteractionPrompt()
    {
        if (_isOpened)
        {
            return string.Empty;
        }

        return "E 유지: 쇠지레로 문 열기";
    }

    public bool CanBeginHold(InteractionContext context)
    {
        if (_isOpened)
        {
            return false;
        }

        if (context.PlayerCondition == null || context.PlayerCondition.IsDead)
        {
            return false;
        }

        if (context.PlayerInventory == null)
        {
            return false;
        }

        bool hasRequiredTool = context.PlayerInventory.HasTool(_requiredToolType);

        if (!hasRequiredTool)
        {
            Debug.Log($"필요한 도구가 없습니다: {_requiredToolType}");
            return false;
        }

        return true;
    }

    public void BeginHold(InteractionContext context)
    {
        _noiseTimer = 0.0f;

        if (_useCrowbarOxygenCost && context.PlayerOxygen != null)
        {
            // 쇠지레 사용 중 추가 산소 소모를 시작한다.
            context.PlayerOxygen.SetExtraConsumePerSecond(context.PlayerOxygen.CrowbarUseConsumePerSecond);
        }

        EmitNoise(context);

        Debug.Log($"문 열기 시작: {_currentProgress:0.0}/{_openDuration:0.0}");
    }

    public bool TickHold(InteractionContext context, float deltaTime)
    {
        if (_isOpened)
        {
            return true;
        }

        if (context.PlayerCondition == null || context.PlayerCondition.IsDead)
        {
            CancelHold(context);
            return false;
        }

        // 누르고 있는 동안 문 열기 진행도를 증가시킨다.
        _currentProgress = Mathf.Clamp(_currentProgress + deltaTime, 0.0f, _openDuration);

        // 일정 간격으로 소음 이벤트를 발생시킨다.
        _noiseTimer += deltaTime;

        if (_noiseTimer >= _noiseInterval)
        {
            _noiseTimer = 0.0f;
            EmitNoise(context);
        }

        Debug.Log($"문 열기 진행도: {GetProgressRatio() * 100.0f:0}%");

        if (_currentProgress < _openDuration)
        {
            return false;
        }

        OpenDoor(context);
        return true;
    }

    public void CancelHold(InteractionContext context)
    {
        if (context.PlayerOxygen != null)
        {
            // 쇠지레 사용 중 추가 산소 소모를 중단한다.
            context.PlayerOxygen.ClearExtraConsumePerSecond();
        }

        Debug.Log($"문 열기 중단: 진행도 {GetProgressRatio() * 100.0f:0}%");
    }

    private void OpenDoor(InteractionContext context)
    {
        _isOpened = true;
        _currentProgress = _openDuration;

        if (context.PlayerOxygen != null)
        {
            // 문 열기 완료 후 추가 산소 소모를 중단한다.
            context.PlayerOxygen.ClearExtraConsumePerSecond();
        }

        Debug.Log("잠긴 문이 열렸습니다.");

        // 열린 문은 더 이상 필요 없으므로 제거한다.
        Destroy(gameObject);
    }

    private void EmitNoise(InteractionContext context)
    {
        Vector2 noisePosition = transform.position;
        GameObject source = context.Interactor != null ? context.Interactor : gameObject;

        NoiseEventData noiseEventData = new NoiseEventData(
            noisePosition,
            _noiseRadius,
            _noiseIntensity,
            source);

        // 적 AI가 나중에 구독할 소음 이벤트를 발생시킨다.
        GameEventBus.RaiseNoiseEmitted(noiseEventData);

        Debug.Log($"소음 발생: 위치 {noisePosition}, 반경 {_noiseRadius:0.0}, 강도 {_noiseIntensity:0.0}");
    }

    private float GetProgressRatio()
    {
        if (_openDuration <= 0.0f)
        {
            return 1.0f;
        }

        return Mathf.Clamp01(_currentProgress / _openDuration);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 문 열기 시간은 0 이하가 되지 않게 제한한다.
        _openDuration = Mathf.Max(0.1f, _openDuration);

        // 소음 값들은 음수가 되지 않게 제한한다.
        _noiseInterval = Mathf.Max(0.1f, _noiseInterval);
        _noiseRadius = Mathf.Max(0.0f, _noiseRadius);
        _noiseIntensity = Mathf.Max(0.0f, _noiseIntensity);
    }

    private void OnDrawGizmos()
    {
        // 쇠지레 사용 시 발생하는 소음 반경을 표시한다.
        Gizmos.color = new Color(1.0f, 0.65f, 0.0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, _noiseRadius);

        Collider2D doorCollider = GetComponent<Collider2D>();

        if (doorCollider == null)
        {
            return;
        }

        // 문 Collider 범위를 표시한다.
        Gizmos.color = new Color(1.0f, 0.2f, 0.2f, 0.6f);
        Gizmos.DrawWireCube(doorCollider.bounds.center, doorCollider.bounds.size);
    }
#endif
}