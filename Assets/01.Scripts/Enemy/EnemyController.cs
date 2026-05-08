using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(EnemyInvestigation))]
[RequireComponent(typeof(EnemyPlayerSensor))]
[RequireComponent(typeof(EnemyChase))]
public sealed class EnemyController : MonoBehaviour
{
    private EnemyMovement _movement;
    private EnemyPatrol _patrol;
    private EnemyInvestigation _investigation;
    private EnemyPlayerSensor _playerSensor;
    private EnemyChase _chase;

    private EnemyState _currentState = EnemyState.Patrol;

    public EnemyState CurrentState => _currentState;

    private void Awake()
    {
        // 같은 GameObject 내부 컴포넌트를 Awake에서 캐싱한다.
        _movement = GetComponent<EnemyMovement>();
        _patrol = GetComponent<EnemyPatrol>();
        _investigation = GetComponent<EnemyInvestigation>();
        _playerSensor = GetComponent<EnemyPlayerSensor>();
        _chase = GetComponent<EnemyChase>();
    }

    private void Update()
    {
        TryBeginChaseBySight();

        switch (_currentState)
        {
            case EnemyState.Patrol:
                _patrol.TickPatrol(Time.deltaTime);
                break;

            case EnemyState.Suspicious:
                _investigation.TickSuspicious(Time.deltaTime);
                break;

            case EnemyState.Approach:
                _investigation.TickApproach(Time.deltaTime);
                break;

            case EnemyState.Chase:
                _chase.TickChase(Time.deltaTime);
                break;

            case EnemyState.Stunned:
                _movement.Stop();
                break;
        }
    }

    public void SetInvestigationTarget(Vector2 targetPosition)
    {
        // 조사 목표 위치를 EnemyInvestigation에 전달한다.
        _investigation.SetInvestigationTarget(targetPosition);
    }

    public void BeginChase(EnemyMoveMode moveMode)
    {
        // 외부 감지 시스템이 추격을 요청할 때 사용한다.
        _chase.SetChaseMoveMode(moveMode);
        ChangeState(EnemyState.Chase);
    }

    public void ChangeState(EnemyState nextState)
    {
        if (_currentState == nextState)
        {
            return;
        }

        // 상태 전환 시 기존 이동을 정리한다.
        _movement.Stop();

        _currentState = nextState;

        switch (_currentState)
        {
            case EnemyState.Patrol:
                _movement.SetMoveMode(EnemyMoveMode.Normal);
                break;

            case EnemyState.Suspicious:
                _movement.SetMoveMode(EnemyMoveMode.Normal);
                _investigation.EnterSuspicious();
                break;

            case EnemyState.Approach:
                _investigation.EnterApproach();
                break;

            case EnemyState.Chase:
                _chase.EnterChase();
                break;
        }

        Debug.Log($"Enemy State Changed: {_currentState}");
    }

    private void TryBeginChaseBySight()
    {
        if (_currentState == EnemyState.Chase || _currentState == EnemyState.Stunned)
        {
            return;
        }

        if (!_playerSensor.CanSeePlayer)
        {
            return;
        }

        // 플레이어를 직접 봤다면 sprint 추격으로 진입한다.
        BeginChase(EnemyMoveMode.Sprint);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 이번 단계에서는 접촉만 로그로 확인한다.
        Debug.Log("적 잠수부가 플레이어와 접촉했습니다.");
    }
}