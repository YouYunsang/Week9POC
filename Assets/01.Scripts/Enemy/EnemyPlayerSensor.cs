using UnityEngine;

public sealed class EnemyPlayerSensor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask _playerLayerMask;
    [SerializeField] private LayerMask _blockingLayerMask;
    [SerializeField] private Transform _sensorPoint;
    [SerializeField] private float _viewDistance = 5.0f;

    private Transform _visiblePlayer;
    private Vector2 _lastSeenPlayerPosition;
    private bool _hasLastSeenPosition;

    public bool CanSeePlayer => _visiblePlayer != null;
    public Vector2 LastSeenPlayerPosition => _lastSeenPlayerPosition;
    public bool HasLastSeenPosition => _hasLastSeenPosition;
    public Transform VisiblePlayer => _visiblePlayer;

    private void Awake()
    {
        if (_sensorPoint == null)
        {
            // 별도 센서 위치가 없으면 적 오브젝트 위치를 기준으로 한다.
            _sensorPoint = transform;
        }
    }

    private void Update()
    {
        // 매 프레임 플레이어를 볼 수 있는지 확인한다.
        UpdatePlayerVisibility();
    }

    private void UpdatePlayerVisibility()
    {
        _visiblePlayer = null;

        Collider2D playerCollider = Physics2D.OverlapCircle(
            _sensorPoint.position,
            _viewDistance,
            _playerLayerMask);

        if (playerCollider == null)
        {
            return;
        }

        Vector2 start = _sensorPoint.position;
        Vector2 end = playerCollider.transform.position;

        if (IsBlocked(start, end))
        {
            return;
        }

        // 벽에 막히지 않았고 거리 안에 있으면 플레이어를 보고 있는 상태다.
        _visiblePlayer = playerCollider.transform;
        _lastSeenPlayerPosition = end;
        _hasLastSeenPosition = true;
    }

    private bool IsBlocked(Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
        {
            return false;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            start,
            direction.normalized,
            distance,
            _blockingLayerMask);

        return hit.collider != null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 감지 거리는 0 이하가 되지 않게 제한한다.
        _viewDistance = Mathf.Max(0.1f, _viewDistance);
    }

    private void OnDrawGizmos()
    {
        Transform center = _sensorPoint != null ? _sensorPoint : transform;

        // 플레이어 직접 감지 거리.
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.35f);
        Gizmos.DrawWireSphere(center.position, _viewDistance);

        // 현재 플레이어를 보고 있다면 시야선을 표시한다.
        if (_visiblePlayer != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center.position, _visiblePlayer.position);
            Gizmos.DrawWireSphere(_visiblePlayer.position, 0.2f);
        }

        // 마지막으로 본 플레이어 위치.
        if (_hasLastSeenPosition)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_lastSeenPlayerPosition, 0.25f);
            Gizmos.DrawLine(center.position, _lastSeenPlayerPosition);
        }
    }
#endif
}