using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public sealed class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private EnemyMovementData _movementData;
    [SerializeField] private Transform[] _waypoints;

    private EnemyMovement _movement;

    private int _currentWaypointIndex;
    private int _patrolDirection = 1;
    private float _lookAroundTimer;
    private bool _isLookingAround;

    public bool HasValidWaypoints => _waypoints != null && _waypoints.Length >= 2;

    private void Awake()
    {
        // 같은 GameObject의 이동 컴포넌트를 캐싱한다.
        _movement = GetComponent<EnemyMovement>();
    }

    public void TickPatrol(float deltaTime)
    {
        if (_movementData == null || !HasValidWaypoints)
        {
            _movement.Stop();
            return;
        }

        _movement.SetMoveMode(EnemyMoveMode.Normal);

        if (_isLookingAround)
        {
            TickLookAround(deltaTime);
            return;
        }

        Transform targetWaypoint = _waypoints[_currentWaypointIndex];

        if (targetWaypoint == null)
        {
            _movement.Stop();
            return;
        }

        Vector2 directionToTarget = targetWaypoint.position - transform.position;

        if (directionToTarget.magnitude <= _movementData.WaypointArriveDistance)
        {
            BeginLookAround();
            return;
        }

        // 현재 Waypoint를 향해 이동한다.
        _movement.SetMoveDirection(directionToTarget.normalized);
    }

    public Vector2 GetClosestWaypointPosition(Vector2 fromPosition)
    {
        if (!HasValidWaypoints)
        {
            return transform.position;
        }

        Transform closestWaypoint = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < _waypoints.Length; i++)
        {
            Transform waypoint = _waypoints[i];

            if (waypoint == null)
            {
                continue;
            }

            float distanceSqr = ((Vector2)waypoint.position - fromPosition).sqrMagnitude;

            if (distanceSqr >= closestDistanceSqr)
            {
                continue;
            }

            closestDistanceSqr = distanceSqr;
            closestWaypoint = waypoint;
            _currentWaypointIndex = i;
        }

        return closestWaypoint != null ? closestWaypoint.position : transform.position;
    }

    private void BeginLookAround()
    {
        // Waypoint에 도착하면 잠시 멈춰 주변을 살핀다.
        _isLookingAround = true;
        _lookAroundTimer = _movementData.LookAroundDuration;
        _movement.Stop();
    }

    private void TickLookAround(float deltaTime)
    {
        _lookAroundTimer -= deltaTime;
        _movement.Stop();

        if (_lookAroundTimer > 0.0f)
        {
            return;
        }

        _isLookingAround = false;
        MoveToNextWaypoint();
    }

    private void MoveToNextWaypoint()
    {
        if (!HasValidWaypoints)
        {
            return;
        }

        _currentWaypointIndex += _patrolDirection;

        if (_currentWaypointIndex >= _waypoints.Length)
        {
            // 마지막 Waypoint에 도달하면 역방향으로 왕복한다.
            _patrolDirection = -1;
            _currentWaypointIndex = _waypoints.Length - 2;
            return;
        }

        if (_currentWaypointIndex < 0)
        {
            // 첫 Waypoint에 도달하면 다시 정방향으로 왕복한다.
            _patrolDirection = 1;
            _currentWaypointIndex = 1;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_waypoints == null || _waypoints.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < _waypoints.Length; i++)
        {
            Transform waypoint = _waypoints[i];

            if (waypoint == null)
            {
                continue;
            }

            // Waypoint 위치를 표시한다.
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(waypoint.position, 0.18f);

            if (i >= _waypoints.Length - 1)
            {
                continue;
            }

            Transform nextWaypoint = _waypoints[i + 1];

            if (nextWaypoint == null)
            {
                continue;
            }

            // Waypoint 순찰 연결선을 표시한다.
            Gizmos.color = new Color(0.0f, 0.8f, 1.0f, 0.8f);
            Gizmos.DrawLine(waypoint.position, nextWaypoint.position);
        }
    }
#endif
}