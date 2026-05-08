using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public sealed class EnemyNoiseSensor : MonoBehaviour
{
    [Header("Suspicion")]
    [SerializeField] private float _suspiciousThreshold = 1.0f;
    [SerializeField] private float _approachThreshold = 3.0f;
    [SerializeField] private float _suspicionDecreasePerSecond = 0.6f;

    [Header("Noise")]
    [SerializeField] private float _noiseSuspicionMultiplier = 1.0f;

    private EnemyController _enemyController;

    private float _currentSuspicion;
    private Vector2 _lastNoisePosition;
    private bool _hasNoisePosition;
    private float _lastNoiseRadius;

    public float CurrentSuspicion => _currentSuspicion;
    public Vector2 LastNoisePosition => _lastNoisePosition;
    public bool HasNoisePosition => _hasNoisePosition;
    public float SuspicionRate => _approachThreshold <= 0.0f ? 0.0f : Mathf.Clamp01(_currentSuspicion / _approachThreshold);

    private void Awake()
    {
        // 같은 GameObject의 EnemyController를 캐싱한다.
        _enemyController = GetComponent<EnemyController>();
    }

    private void OnEnable()
    {
        // 쇠지레 등에서 발생하는 소음 이벤트를 구독한다.
        GameEventBus.NoiseEmitted += HandleNoiseEmitted;
    }

    private void OnDisable()
    {
        // 비활성화 시 이벤트 구독을 해제한다.
        GameEventBus.NoiseEmitted -= HandleNoiseEmitted;
    }

    private void Update()
    {
        TickSuspicionDecay(Time.deltaTime);
    }

    public void ClearSuspicion()
    {
        // 탐색 완료 또는 Patrol 복귀 시 의심도를 초기화한다.
        _currentSuspicion = 0.0f;
        _hasNoisePosition = false;
    }

    private void HandleNoiseEmitted(NoiseEventData noiseEventData)
    {
        float distance = Vector2.Distance(transform.position, noiseEventData.Position);

        if (distance > noiseEventData.Radius)
        {
            return;
        }

        // 마지막으로 들은 소음 위치를 항상 갱신한다.
        _lastNoisePosition = noiseEventData.Position;
        _lastNoiseRadius = noiseEventData.Radius;
        _hasNoisePosition = true;

        // 소음 강도를 의심도 증가량으로 사용한다.
        float suspicionAmount = noiseEventData.Intensity * _noiseSuspicionMultiplier;
        _currentSuspicion += suspicionAmount;

        Debug.Log($"적이 소음을 들었습니다. 의심도: {_currentSuspicion:0.0}, 위치: {_lastNoisePosition}");

        if (_currentSuspicion >= _approachThreshold)
        {
            // 의심도가 충분히 쌓이면 마지막 소음 위치로 접근한다.
            _enemyController.SetInvestigationTarget(_lastNoisePosition);
            _enemyController.ChangeState(EnemyState.Approach);
            return;
        }

        if (_currentSuspicion >= _suspiciousThreshold)
        {
            // 낮은 단계의 의심은 소음 위치를 바라보는 상태로 처리한다.
            _enemyController.SetInvestigationTarget(_lastNoisePosition);
            _enemyController.ChangeState(EnemyState.Suspicious);
        }
    }

    private void TickSuspicionDecay(float deltaTime)
    {
        if (_currentSuspicion <= 0.0f)
        {
            return;
        }

        if (_enemyController.CurrentState == EnemyState.Approach)
        {
            return;
        }

        // 쇠지레 소음이 멈추면 의심도는 천천히 감소한다.
        _currentSuspicion = Mathf.Max(
            0.0f,
            _currentSuspicion - _suspicionDecreasePerSecond * deltaTime);

        if (_currentSuspicion > 0.0f)
        {
            return;
        }

        if (_enemyController.CurrentState == EnemyState.Suspicious)
        {
            // 의심이 완전히 풀리면 순찰로 복귀한다.
            _enemyController.ChangeState(EnemyState.Patrol);
        }

        _hasNoisePosition = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 의심도 임계값은 0 이하가 되지 않게 제한한다.
        _suspiciousThreshold = Mathf.Max(0.01f, _suspiciousThreshold);
        _approachThreshold = Mathf.Max(_suspiciousThreshold, _approachThreshold);

        // 의심도 감소량과 배율은 음수가 되지 않게 제한한다.
        _suspicionDecreasePerSecond = Mathf.Max(0.0f, _suspicionDecreasePerSecond);
        _noiseSuspicionMultiplier = Mathf.Max(0.0f, _noiseSuspicionMultiplier);
    }

    private void OnDrawGizmos()
    {
        if (!_hasNoisePosition)
        {
            return;
        }

        // 마지막으로 감지한 소음 반경.
        Gizmos.color = new Color(1.0f, 0.45f, 0.0f, 0.35f);
        Gizmos.DrawWireSphere(_lastNoisePosition, _lastNoiseRadius);

        // 마지막 소음 위치.
        Gizmos.color = new Color(1.0f, 0.8f, 0.0f, 0.9f);
        Gizmos.DrawWireSphere(_lastNoisePosition, 0.25f);

        // 적에서 소음 위치까지의 연결선.
        Gizmos.color = new Color(1.0f, 0.55f, 0.0f, 0.9f);
        Gizmos.DrawLine(transform.position, _lastNoisePosition);
    }
#endif
}