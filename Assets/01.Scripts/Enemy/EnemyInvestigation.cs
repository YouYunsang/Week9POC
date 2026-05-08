using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyNoiseSensor))]
public sealed class EnemyInvestigation : MonoBehaviour
{
    [Header("Suspicious")]
    [SerializeField] private float _lookAtNoiseDuration = 0.8f;

    [Header("Approach")]
    [SerializeField] private float _arriveDistance = 0.25f;
    [SerializeField] private float _searchDuration = 2.0f;

    private EnemyMovement _movement;
    private EnemyController _controller;
    private EnemyNoiseSensor _noiseSensor;

    private Vector2 _investigationTarget;
    private float _suspiciousTimer;
    private float _searchTimer;
    private bool _isSearching;

    public Vector2 InvestigationTarget => _investigationTarget;

    private void Awake()
    {
        // 같은 GameObject 내부 컴포넌트를 Awake에서 캐싱한다.
        _movement = GetComponent<EnemyMovement>();
        _controller = GetComponent<EnemyController>();
        _noiseSensor = GetComponent<EnemyNoiseSensor>();
    }

    public void SetInvestigationTarget(Vector2 targetPosition)
    {
        // 마지막 소음 위치를 조사 목표로 저장한다.
        _investigationTarget = targetPosition;

        // 새 소음이 들어오면 접근 후 탐색 상태를 초기화한다.
        _isSearching = false;
        _searchTimer = 0.0f;
    }

    public void EnterSuspicious()
    {
        // Suspicious 상태에 들어오면 잠시 소음 위치를 바라본다.
        _suspiciousTimer = _lookAtNoiseDuration;
        _movement.Stop();

        FaceTarget(_investigationTarget);

        Debug.Log($"적이 소음 위치를 바라봅니다: {_investigationTarget}");
    }

    public void TickSuspicious(float deltaTime)
    {
        _movement.Stop();
        FaceTarget(_investigationTarget);

        _suspiciousTimer -= deltaTime;

        if (_suspiciousTimer > 0.0f)
        {
            return;
        }

        // 의심도가 아직 접근 임계값에 도달하지 않았다면 NoiseSensor의 감쇠에 맡긴다.
        // 여기서는 별도 상태 전환을 강제하지 않는다.
    }

    public void EnterApproach()
    {
        // Approach 상태에 들어오면 소음 위치로 이동할 준비를 한다.
        _isSearching = false;
        _searchTimer = 0.0f;

        Debug.Log($"적이 소음 위치로 접근합니다: {_investigationTarget}");
    }

    public void TickApproach(float deltaTime)
    {
        if (_isSearching)
        {
            TickSearch(deltaTime);
            return;
        }

        Vector2 directionToTarget = _investigationTarget - (Vector2)transform.position;

        if (directionToTarget.magnitude <= _arriveDistance)
        {
            BeginSearch();
            return;
        }

        EnemyMoveMode moveMode = _noiseSensor.SuspicionRate >= 0.5f ? EnemyMoveMode.Sprint : EnemyMoveMode.Normal;

        _movement.SetMoveMode(moveMode);

        // 마지막 소음 위치로 이동한다.
        _movement.SetMoveDirection(directionToTarget.normalized);
        FaceTarget(_investigationTarget);
    }

    private void BeginSearch()
    {
        // 소음 위치에 도착하면 잠시 주변을 탐색한다.
        _isSearching = true;
        _searchTimer = _searchDuration;
        _movement.Stop();

        Debug.Log("적이 소음 위치 주변을 탐색합니다.");
    }

    private void TickSearch(float deltaTime)
    {
        _movement.Stop();

        _searchTimer -= deltaTime;

        if (_searchTimer > 0.0f)
        {
            return;
        }

        // 아무것도 찾지 못하면 의심을 해제하고 순찰로 복귀한다.
        _noiseSensor.ClearSuspicion();
        _controller.ChangeState(EnemyState.Patrol);

        Debug.Log("적이 아무것도 찾지 못하고 순찰로 복귀합니다.");
    }

    private void FaceTarget(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        // 임시 방향 전환: Sprite가 오른쪽을 기본 방향으로 본다고 가정한다.
        float sign = direction.x >= 0.0f ? 1.0f : -1.0f;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * sign;
        transform.localScale = localScale;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 의심 대기와 탐색 시간은 음수가 되지 않게 제한한다.
        _lookAtNoiseDuration = Mathf.Max(0.0f, _lookAtNoiseDuration);
        _arriveDistance = Mathf.Max(0.01f, _arriveDistance);
        _searchDuration = Mathf.Max(0.0f, _searchDuration);
    }

    private void OnDrawGizmos()
    {
        // 조사 목표 위치를 표시한다.
        Gizmos.color = new Color(1.0f, 0.9f, 0.0f, 0.9f);
        Gizmos.DrawWireSphere(_investigationTarget, 0.22f);

        // 조사 목표 도착 판정 범위를 표시한다.
        Gizmos.color = new Color(1.0f, 0.9f, 0.0f, 0.35f);
        Gizmos.DrawWireSphere(_investigationTarget, _arriveDistance);

        // 적에서 조사 목표까지의 연결선.
        Gizmos.color = new Color(1.0f, 0.9f, 0.0f, 0.7f);
        Gizmos.DrawLine(transform.position, _investigationTarget);
    }
#endif
}