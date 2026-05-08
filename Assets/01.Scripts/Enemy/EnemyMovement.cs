using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyMovement : MonoBehaviour
{
    [SerializeField] private EnemyMovementData _movementData;

    private Rigidbody2D _rigidbody;
    private Vector2 _moveDirection;
    private EnemyMoveMode _moveMode = EnemyMoveMode.Normal;

    private const float DIRECTION_EPSILON = 0.01f;

    public Vector2 CurrentVelocity => _rigidbody != null ? _rigidbody.linearVelocity : Vector2.zero;
    public EnemyMoveMode MoveMode => _moveMode;

    private void Awake()
    {
        // Rigidbody2D는 Awake에서 캐싱한다.
        _rigidbody = GetComponent<Rigidbody2D>();

        // 수중 이동이므로 중력은 사용하지 않는다.
        _rigidbody.gravityScale = 0.0f;

        // 적 잠수부가 충돌로 회전하지 않도록 고정한다.
        _rigidbody.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (_movementData == null)
        {
            return;
        }

        // 현재 이동 모드에 따라 목표 속도를 결정한다.
        float speed = _moveMode == EnemyMoveMode.Sprint
            ? _movementData.SprintMoveSpeed
            : _movementData.MoveSpeed;

        Vector2 targetVelocity = _moveDirection.normalized * speed;

        // 수중 관성 이동을 적용한다.
        Vector2 nextVelocity = CalculateNextVelocity(targetVelocity);

        // Rigidbody2D에 최종 속도를 적용한다.
        _rigidbody.linearVelocity = nextVelocity;
    }

    public void SetMoveDirection(Vector2 moveDirection)
    {
        // 외부 AI 로직이 지정한 이동 방향을 저장한다.
        _moveDirection = Vector2.ClampMagnitude(moveDirection, 1.0f);
    }

    public void SetMoveMode(EnemyMoveMode moveMode)
    {
        // 적의 일반 이동 / sprint 이동 모드를 설정한다.
        _moveMode = moveMode;
    }

    public void Stop()
    {
        // 순찰 대기나 상태 전환 시 이동 방향을 제거한다.
        _moveDirection = Vector2.zero;
    }

    private Vector2 CalculateNextVelocity(Vector2 targetVelocity)
    {
        Vector2 currentVelocity = _rigidbody.linearVelocity;

        bool hasMoveDirection = _moveDirection.sqrMagnitude > DIRECTION_EPSILON;
        float acceleration = hasMoveDirection
            ? _movementData.Acceleration
            : _movementData.Deceleration;

        if (IsChangingDirection(currentVelocity, targetVelocity))
        {
            // 방향을 바꿀 때 수중 저항 때문에 즉시 꺾이지 않게 만든다.
            acceleration *= _movementData.DirectionChangeResistanceMultiplier;
        }

        float nextX = Mathf.MoveTowards(
            currentVelocity.x,
            targetVelocity.x,
            acceleration * Time.fixedDeltaTime);

        float nextY = Mathf.MoveTowards(
            currentVelocity.y,
            targetVelocity.y,
            acceleration * Time.fixedDeltaTime);

        return new Vector2(nextX, nextY);
    }

    private bool IsChangingDirection(Vector2 currentVelocity, Vector2 targetVelocity)
    {
        if (currentVelocity.sqrMagnitude < DIRECTION_EPSILON)
        {
            return false;
        }

        if (targetVelocity.sqrMagnitude < DIRECTION_EPSILON)
        {
            return false;
        }

        // 현재 속도와 목표 속도의 방향이 반대에 가까운지 확인한다.
        return Vector2.Dot(currentVelocity.normalized, targetVelocity.normalized) < 0.0f;
    }
}