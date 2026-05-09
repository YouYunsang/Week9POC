using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInventory))]
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerMovementData _movementData;

    private Rigidbody2D _rigidbody;
    private PlayerInventory _inventory;
    private Vector2 _moveInput;
    private bool _isSprinting;
    private bool _isBuoyancyEnabled = true;

    private const float DIRECTION_EPSILON = 0.01f;

    private void Awake()
    {
        // Rigidbody2D는 Awake에서 캐싱한다.
        _rigidbody = GetComponent<Rigidbody2D>();
        _inventory = GetComponent<PlayerInventory>();

        // 수중 이동이므로 중력은 사용하지 않는다.
        _rigidbody.gravityScale = 0.0f;

        // 잠수부 캐릭터가 충돌로 회전하지 않도록 고정한다.
        _rigidbody.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (_movementData == null)
        {
            return;
        }

        // 현재 입력을 수중 이동용 입력으로 보정한다.
        Vector2 processedInput = GetProcessedInput(_moveInput);

        // 보정된 입력을 기준으로 목표 속도를 계산한다.
        Vector2 targetVelocity = CalculateTargetVelocity(processedInput);

        // 현재 속도에서 목표 속도로 천천히 이동한다.
        Vector2 nextVelocity = CalculateNextVelocity(targetVelocity, processedInput);

        // Rigidbody2D에 최종 속도를 적용한다.
        _rigidbody.linearVelocity = nextVelocity;
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        // 외부 입력을 Movement 내부 상태로 저장한다.
        _moveInput = moveInput;
    }

    public void SetSprintState(bool isSprinting)
    {
        // 빠른 헤엄 상태를 저장한다.
        _isSprinting = isSprinting;
    }

    private Vector2 GetProcessedInput(Vector2 rawInput)
    {
        // 작은 입력 흔들림을 제거한다.
        if (rawInput.sqrMagnitude < _movementData.InputDeadZone * _movementData.InputDeadZone)
        {
            return Vector2.zero;
        }

        // 대각선 이동이 과하게 빨라지지 않도록 정규화한다.
        return Vector2.ClampMagnitude(rawInput, 1.0f);
    }

    private Vector2 CalculateTargetVelocity(Vector2 input)
    {
        float speed = _movementData.BaseMoveSpeed;

        if (_isSprinting)
        {
            // 빠른 헤엄은 대시가 아니라 기본 속도의 제한적 증가로 처리한다.
            speed *= _movementData.SprintSpeedMultiplier;
        }

        float targetX = input.x * speed;
        float targetY = CalculateTargetVerticalVelocity(input.y, speed);

        return new Vector2(targetX, targetY);
    }

    private float CalculateTargetVerticalVelocity(float verticalInput, float speed)
    {
        if (verticalInput > _movementData.InputDeadZone)
        {
            // 위로 이동할 때도 수중 저항 때문에 기본 속도보다 둔하게 만든다.
            return verticalInput * speed * _movementData.UpwardSpeedMultiplier;
        }

        if (verticalInput < -_movementData.InputDeadZone)
        {
            // 아래로 잠수하는 입력은 부력에 저항하는 느낌을 위해 더 둔하게 만든다.
            return verticalInput * speed * _movementData.DownwardSpeedMultiplier;
        }

        if (!_isBuoyancyEnabled)
        {
            return 0.0f;
        }

        // 수직 입력이 없으면 몸이 조금씩 떠오른다.
        return _movementData.IdleBuoyancySpeed * CalculateBuoyancyWeightMultiplier();
    }

    private float CalculateBuoyancyWeightMultiplier()
    {
        if (_inventory == null)
        {
            return 1.0f;
        }

        return 1.0f - _inventory.WeightRatio;
    }

    private Vector2 CalculateNextVelocity(Vector2 targetVelocity, Vector2 input)
    {
        Vector2 currentVelocity = _rigidbody.linearVelocity;

        float xAcceleration = CalculateAxisAcceleration(
            currentVelocity.x,
            targetVelocity.x,
            input.x,
            _movementData.HorizontalAcceleration);

        float yAcceleration = CalculateVerticalAcceleration(
            currentVelocity.y,
            targetVelocity.y,
            input.y);

        float nextX = Mathf.MoveTowards(
            currentVelocity.x,
            targetVelocity.x,
            xAcceleration * Time.fixedDeltaTime);

        float nextY = Mathf.MoveTowards(
            currentVelocity.y,
            targetVelocity.y,
            yAcceleration * Time.fixedDeltaTime);

        return new Vector2(nextX, nextY);
    }

    private float CalculateAxisAcceleration(
        float currentVelocity,
        float targetVelocity,
        float input,
        float baseAcceleration)
    {
        bool hasInput = Mathf.Abs(input) > _movementData.InputDeadZone;
        float acceleration = hasInput ? baseAcceleration : _movementData.Deceleration;

        if (_isSprinting && hasInput)
        {
            // 빠른 헤엄 중에는 더 빠르지만 제어가 둔해지도록 가속력을 낮춘다.
            acceleration *= _movementData.SprintAccelerationMultiplier;
        }

        if (IsChangingDirection(currentVelocity, targetVelocity))
        {
            // 반대 방향 전환 시 물 저항 때문에 즉시 꺾이지 않게 만든다.
            acceleration *= _movementData.DirectionChangeResistanceMultiplier;
        }

        return acceleration;
    }

    private float CalculateVerticalAcceleration(
        float currentVelocity,
        float targetVelocity,
        float verticalInput)
    {
        bool hasVerticalInput = Mathf.Abs(verticalInput) > _movementData.InputDeadZone;

        if (!hasVerticalInput)
        {
            // 입력이 없을 때의 상승은 부력 전용 가속도로 천천히 처리한다.
            return _movementData.BuoyancyAcceleration;
        }

        return CalculateAxisAcceleration(
            currentVelocity,
            targetVelocity,
            verticalInput,
            _movementData.VerticalAcceleration);
    }

    private bool IsChangingDirection(float currentVelocity, float targetVelocity)
    {
        if (Mathf.Abs(currentVelocity) < DIRECTION_EPSILON)
        {
            return false;
        }

        if (Mathf.Abs(targetVelocity) < DIRECTION_EPSILON)
        {
            return false;
        }

        // 현재 이동 방향과 목표 이동 방향이 반대인지 확인한다.
        return Mathf.Sign(currentVelocity) != Mathf.Sign(targetVelocity);
    }

    public void SetBuoyancyEnabled(bool isEnabled)
    {
        _isBuoyancyEnabled = isEnabled;
    }
}
