using UnityEngine;

public sealed class GameFlowManager : MonoBehaviour
{
    private GameFlowState _currentState = GameFlowState.Exploring;

    public GameFlowState CurrentState => _currentState;

    private void OnEnable()
    {
        // 게임 진행 관련 이벤트를 구독한다.
        GameEventBus.ReturnToBaseRequested += HandleReturnToBaseRequested;
        GameEventBus.PlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        // 비활성화 시 이벤트 구독을 해제한다.
        GameEventBus.ReturnToBaseRequested -= HandleReturnToBaseRequested;
        GameEventBus.PlayerDied -= HandlePlayerDied;
    }

    private void HandleReturnToBaseRequested()
    {
        if (_currentState != GameFlowState.Exploring)
        {
            return;
        }

        // 플레이어가 살아서 복귀 지점에서 상호작용했으므로 탐색을 성공 종료한다.
        EndRun(GameResultType.ReturnedToBase);
    }

    private void HandlePlayerDied()
    {
        if (_currentState != GameFlowState.Exploring)
        {
            return;
        }

        // 산소 고갈 등으로 플레이어가 사망했으므로 탐색을 실패 종료한다.
        EndRun(GameResultType.Drowned);
    }

    private void EndRun(GameResultType resultType)
    {
        _currentState = GameFlowState.Ended;

        // 다른 시스템이 탐색 종료를 알 수 있도록 이벤트를 발생시킨다.
        GameEventBus.RaiseRunEnded(resultType);

        Debug.Log($"탐색 종료: {resultType}");
    }
}

public enum GameFlowState
{
    Exploring = 0,
    Ended = 1
}