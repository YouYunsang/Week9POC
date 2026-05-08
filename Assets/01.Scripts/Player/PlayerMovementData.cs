using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerMovementData",
    menuName = "ScriptableObjects/Player/Player Movement Data")]
public sealed class PlayerMovementData : ScriptableObject
{
    [Header("Base Movement")]
    [SerializeField] private float _baseMoveSpeed = 2.2f;
    [SerializeField] private float _sprintSpeedMultiplier = 2f;

    [Header("Acceleration")]
    [SerializeField] private float _horizontalAcceleration = 5.0f;
    [SerializeField] private float _verticalAcceleration = 3.2f;
    [SerializeField] private float _deceleration = 2.4f;
    [SerializeField] private float _sprintAccelerationMultiplier = 0.75f;

    [Header("Underwater Resistance")]
    [SerializeField] private float _directionChangeResistanceMultiplier = 0.45f;
    [SerializeField] private float _downwardSpeedMultiplier = 0.65f;
    [SerializeField] private float _upwardSpeedMultiplier = 0.85f;

    [Header("Buoyancy")]
    [SerializeField] private float _idleBuoyancySpeed = 0.28f;
    [SerializeField] private float _buoyancyAcceleration = 1.1f;

    [Header("Input")]
    [SerializeField] private float _inputDeadZone = 0.1f;

    public float BaseMoveSpeed => _baseMoveSpeed;
    public float SprintSpeedMultiplier => _sprintSpeedMultiplier;

    public float HorizontalAcceleration => _horizontalAcceleration;
    public float VerticalAcceleration => _verticalAcceleration;
    public float Deceleration => _deceleration;
    public float SprintAccelerationMultiplier => _sprintAccelerationMultiplier;

    public float DirectionChangeResistanceMultiplier => _directionChangeResistanceMultiplier;
    public float DownwardSpeedMultiplier => _downwardSpeedMultiplier;
    public float UpwardSpeedMultiplier => _upwardSpeedMultiplier;

    public float IdleBuoyancySpeed => _idleBuoyancySpeed;
    public float BuoyancyAcceleration => _buoyancyAcceleration;

    public float InputDeadZone => _inputDeadZone;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 음수 수치 입력을 방지해 런타임 이동 계산 오류를 막는다.
        _baseMoveSpeed = Mathf.Max(0.01f, _baseMoveSpeed);
        _sprintSpeedMultiplier = Mathf.Max(1.0f, _sprintSpeedMultiplier);

        _horizontalAcceleration = Mathf.Max(0.01f, _horizontalAcceleration);
        _verticalAcceleration = Mathf.Max(0.01f, _verticalAcceleration);
        _deceleration = Mathf.Max(0.01f, _deceleration);
        _sprintAccelerationMultiplier = Mathf.Clamp(_sprintAccelerationMultiplier, 0.1f, 1.0f);

        _directionChangeResistanceMultiplier = Mathf.Clamp(_directionChangeResistanceMultiplier, 0.1f, 1.0f);
        _downwardSpeedMultiplier = Mathf.Clamp(_downwardSpeedMultiplier, 0.1f, 1.0f);
        _upwardSpeedMultiplier = Mathf.Clamp(_upwardSpeedMultiplier, 0.1f, 1.0f);

        _idleBuoyancySpeed = Mathf.Max(0.0f, _idleBuoyancySpeed);
        _buoyancyAcceleration = Mathf.Max(0.01f, _buoyancyAcceleration);

        _inputDeadZone = Mathf.Clamp01(_inputDeadZone);
    }
#endif
}