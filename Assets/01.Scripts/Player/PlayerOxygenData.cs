using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerOxygenData",
    menuName = "ScriptableObjects/Player/Player Oxygen Data")]
public sealed class PlayerOxygenData : ScriptableObject
{
    [Header("Oxygen")]
    [SerializeField] private float _maxOxygen = 90.0f;

    [Header("Consume Per Second")]
    [SerializeField] private float _idleConsumePerSecond = 0.45f;
    [SerializeField] private float _moveConsumePerSecond = 0.8f;
    [SerializeField] private float _sprintConsumePerSecond = 1.6f;

    [Header("Action Consume Per Second")]
    [SerializeField] private float _crowbarUseConsumePerSecond = 1.2f;

    [Header("Instant Consume")]
    [SerializeField] private float _defaultInstantConsumeAmount = 5.0f;

    public float MaxOxygen => _maxOxygen;
    public float IdleConsumePerSecond => _idleConsumePerSecond;
    public float MoveConsumePerSecond => _moveConsumePerSecond;
    public float SprintConsumePerSecond => _sprintConsumePerSecond;
    public float CrowbarUseConsumePerSecond => _crowbarUseConsumePerSecond;
    public float DefaultInstantConsumeAmount => _defaultInstantConsumeAmount;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 산소 최대량은 0 이하가 되면 게임 루프가 성립하지 않으므로 최소값을 보장한다.
        _maxOxygen = Mathf.Max(1.0f, _maxOxygen);

        // 초당 소모량은 음수가 되면 산소가 회복되는 버그가 생기므로 0 이상으로 제한한다.
        _idleConsumePerSecond = Mathf.Max(0.0f, _idleConsumePerSecond);
        _moveConsumePerSecond = Mathf.Max(0.0f, _moveConsumePerSecond);
        _sprintConsumePerSecond = Mathf.Max(0.0f, _sprintConsumePerSecond);
        _crowbarUseConsumePerSecond = Mathf.Max(0.0f, _crowbarUseConsumePerSecond);

        // 즉시 소모량도 음수가 되지 않도록 제한한다.
        _defaultInstantConsumeAmount = Mathf.Max(0.0f, _defaultInstantConsumeAmount);
    }
#endif
}