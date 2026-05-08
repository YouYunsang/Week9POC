using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyPlayerSensor))]
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(EnemyController))]
public sealed class EnemyChase : MonoBehaviour
{
    [Header("Chase")]
    [SerializeField] private float _lostSightDuration = 2.0f;
    [SerializeField] private float _arriveDistance = 0.25f;

    [Header("Search")]
    [SerializeField] private float _searchDuration = 2.0f;

    private EnemyMovement _movement;
    private EnemyPlayerSensor _playerSensor;
    private EnemyPatrol _patrol;
    private EnemyController _controller;

    private EnemyMoveMode _chaseMoveMode = EnemyMoveMode.Sprint;
    private EnemyChasePhase _phase = EnemyChasePhase.PursuePlayer;

    private Vector2 _lastKnownPlayerPosition;
    private Vector2 _returnWaypointPosition;

    private float _lostSightTimer;
    private float _searchTimer;

    private enum EnemyChasePhase
    {
        PursuePlayer = 0,
        MoveToLastKnownPosition = 1,
        SearchLastKnownPosition = 2,
        ReturnToWaypoint = 3
    }

    private void Awake()
    {
        // 같은 GameObject 내부 컴포넌트를 Awake에서 캐싱한다.
        _movement = GetComponent<EnemyMovement>();
        _playerSensor = GetComponent<EnemyPlayerSensor>();
        _patrol = GetComponent<EnemyPatrol>();
        _controller = GetComponent<EnemyController>();
    }

    public void SetChaseMoveMode(EnemyMoveMode moveMode)
    {
        // 추격 속도 모드를 설정한다.
        _chaseMoveMode = moveMode;
    }

    public void EnterChase()
    {
        // Chase 진입 시 플레이어 추적 단계로 시작한다.
        _phase = EnemyChasePhase.PursuePlayer;
        _lostSightTimer = _lostSightDuration;
        _searchTimer = 0.0f;

        if (_playerSensor.HasLastSeenPosition)
        {
            _lastKnownPlayerPosition = _playerSensor.LastSeenPlayerPosition;
        }
        else
        {
            _lastKnownPlayerPosition = transform.position;
        }

        Debug.Log($"적이 추격을 시작합니다. 속도 모드: {_chaseMoveMode}");
    }

    public void TickChase(float deltaTime)
    {
        switch (_phase)
        {
            case EnemyChasePhase.PursuePlayer:
                TickPursuePlayer(deltaTime);
                break;

            case EnemyChasePhase.MoveToLastKnownPosition:
                TickMoveToLastKnownPosition();
                break;

            case EnemyChasePhase.SearchLastKnownPosition:
                TickSearchLastKnownPosition(deltaTime);
                break;

            case EnemyChasePhase.ReturnToWaypoint:
                TickReturnToWaypoint();
                break;
        }
    }

    private void TickPursuePlayer(float deltaTime)
    {
        _movement.SetMoveMode(_chaseMoveMode);

        if (_playerSensor.CanSeePlayer)
        {
            // 플레이어가 보이면 현재 위치를 계속 추적한다.
            _lastKnownPlayerPosition = _playerSensor.VisiblePlayer.position;
            _lostSightTimer = _lostSightDuration;

            MoveTo(_lastKnownPlayerPosition);
            return;
        }

        // 플레이어가 안 보이면 놓침 타이머를 줄인다.
        _lostSightTimer -= deltaTime;

        if (_lostSightTimer > 0.0f)
        {
            // 잠깐 놓친 정도라면 마지막 위치로 계속 이동한다.
            MoveTo(_lastKnownPlayerPosition);
            return;
        }

        // 일정 시간 못 보면 마지막으로 본 위치까지 이동하는 단계로 전환한다.
        _phase = EnemyChasePhase.MoveToLastKnownPosition;
        Debug.Log("적이 플레이어를 놓쳤습니다. 마지막 위치로 이동합니다.");
    }

    private void TickMoveToLastKnownPosition()
    {
        _movement.SetMoveMode(EnemyMoveMode.Normal);

        if (IsArrived(_lastKnownPlayerPosition))
        {
            BeginSearchLastKnownPosition();
            return;
        }

        MoveTo(_lastKnownPlayerPosition);
    }

    private void BeginSearchLastKnownPosition()
    {
        _phase = EnemyChasePhase.SearchLastKnownPosition;
        _searchTimer = _searchDuration;
        _movement.Stop();

        Debug.Log("적이 마지막 플레이어 위치를 탐색합니다.");
    }

    private void TickSearchLastKnownPosition(float deltaTime)
    {
        _movement.Stop();

        if (_playerSensor.CanSeePlayer)
        {
            // 탐색 중 다시 플레이어를 보면 즉시 추격으로 복귀한다.
            _phase = EnemyChasePhase.PursuePlayer;
            _lostSightTimer = _lostSightDuration;
            return;
        }

        _searchTimer -= deltaTime;

        if (_searchTimer > 0.0f)
        {
            return;
        }

        BeginReturnToWaypoint();
    }

    private void BeginReturnToWaypoint()
    {
        _phase = EnemyChasePhase.ReturnToWaypoint;
        _returnWaypointPosition = _patrol.GetClosestWaypointPosition(transform.position);

        Debug.Log("적이 가장 가까운 순찰 지점으로 복귀합니다.");
    }

    private void TickReturnToWaypoint()
    {
        _movement.SetMoveMode(EnemyMoveMode.Normal);

        if (_playerSensor.CanSeePlayer)
        {
            // 복귀 중 플레이어를 다시 보면 추격을 재개한다.
            _phase = EnemyChasePhase.PursuePlayer;
            _lostSightTimer = _lostSightDuration;
            return;
        }

        if (IsArrived(_returnWaypointPosition))
        {
            _movement.Stop();
            _controller.ChangeState(EnemyState.Patrol);
            return;
        }

        MoveTo(_returnWaypointPosition);
    }

    private void MoveTo(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
            _movement.Stop();
            return;
        }

        _movement.SetMoveDirection(direction.normalized);
        FaceTarget(targetPosition);
    }

    private bool IsArrived(Vector2 targetPosition)
    {
        return Vector2.Distance(transform.position, targetPosition) <= _arriveDistance;
    }

    private void FaceTarget(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        // Sprite가 오른쪽을 기본 방향으로 본다고 가정한다.
        float sign = direction.x >= 0.0f ? 1.0f : -1.0f;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * sign;
        transform.localScale = localScale;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _lostSightDuration = Mathf.Max(0.1f, _lostSightDuration);
        _arriveDistance = Mathf.Max(0.01f, _arriveDistance);
        _searchDuration = Mathf.Max(0.0f, _searchDuration);
    }

    private void OnDrawGizmos()
    {
        // 마지막으로 본 플레이어 위치.
        Gizmos.color = new Color(1.0f, 0.2f, 0.2f, 0.9f);
        Gizmos.DrawWireSphere(_lastKnownPlayerPosition, 0.25f);

        // 마지막 위치 도착 판정 범위.
        Gizmos.color = new Color(1.0f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(_lastKnownPlayerPosition, _arriveDistance);

        // 가장 가까운 복귀 Waypoint 위치.
        Gizmos.color = new Color(0.2f, 1.0f, 0.3f, 0.9f);
        Gizmos.DrawWireSphere(_returnWaypointPosition, 0.25f);

        // 복귀 Waypoint 도착 판정 범위.
        Gizmos.color = new Color(0.2f, 1.0f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(_returnWaypointPosition, _arriveDistance);

        // 현재 Chase 단계별 목표선.
        Vector2 currentTarget = GetCurrentGizmoTarget();

        Gizmos.color = new Color(1.0f, 0.1f, 0.1f, 0.8f);
        Gizmos.DrawLine(transform.position, currentTarget);
    }

    private Vector2 GetCurrentGizmoTarget()
    {
        switch (_phase)
        {
            case EnemyChasePhase.PursuePlayer:
            case EnemyChasePhase.MoveToLastKnownPosition:
            case EnemyChasePhase.SearchLastKnownPosition:
                return _lastKnownPlayerPosition;

            case EnemyChasePhase.ReturnToWaypoint:
                return _returnWaypointPosition;

            default:
                return transform.position;
        }
    }
#endif
}