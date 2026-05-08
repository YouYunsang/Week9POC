using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyMovementData",
    menuName = "ScriptableObjects/Enemy/Enemy Movement Data")]
public sealed class EnemyMovementData : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 1.7f;
    [SerializeField] private float _sprintMoveSpeed = 2.4f;
    [SerializeField] private float _acceleration = 3.0f;
    [SerializeField] private float _deceleration = 2.0f;
    [SerializeField] private float _directionChangeResistanceMultiplier = 0.5f;

    [Header("Patrol")]
    [SerializeField] private float _waypointArriveDistance = 0.15f;
    [SerializeField] private float _lookAroundDuration = 1.0f;

    public float MoveSpeed => _moveSpeed;
    public float SprintMoveSpeed => _sprintMoveSpeed;
    public float Acceleration => _acceleration;
    public float Deceleration => _deceleration;
    public float DirectionChangeResistanceMultiplier => _directionChangeResistanceMultiplier;
    public float WaypointArriveDistance => _waypointArriveDistance;
    public float LookAroundDuration => _lookAroundDuration;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 적 기본 속도는 0 이하가 되지 않게 제한한다.
        _moveSpeed = Mathf.Max(0.01f, _moveSpeed);

        // 적 sprint는 기본 속도보다 느리지 않게 제한한다.
        _sprintMoveSpeed = Mathf.Max(_moveSpeed, _sprintMoveSpeed);

        // 가속/감속 값은 음수가 되지 않게 제한한다.
        _acceleration = Mathf.Max(0.01f, _acceleration);
        _deceleration = Mathf.Max(0.01f, _deceleration);

        // 방향 전환 저항은 과도한 값을 막는다.
        _directionChangeResistanceMultiplier = Mathf.Clamp(
            _directionChangeResistanceMultiplier,
            0.1f,
            1.0f);

        _waypointArriveDistance = Mathf.Max(0.01f, _waypointArriveDistance);
        _lookAroundDuration = Mathf.Max(0.0f, _lookAroundDuration);
    }
#endif
}