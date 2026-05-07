using System;
using UnityEngine;

public sealed class PlayerCondition : MonoBehaviour
{
    private bool _isDead;
    private bool _isInputBlocked;
    private bool _isMovementBlocked;
    private bool _isInteractionBlocked;

    public event Action PlayerDied;

    public bool IsDead => _isDead;
    public bool CanReceiveInput => !_isDead && !_isInputBlocked;
    public bool CanMove => !_isDead && !_isMovementBlocked;
    public bool CanInteract => !_isDead && !_isInteractionBlocked;

    private void OnEnable()
    {
        // 탐색이 종료되면 플레이어 조작을 막는다.
        GameEventBus.RunEnded += HandleRunEnded;
    }

    private void OnDisable()
    {
        GameEventBus.RunEnded -= HandleRunEnded;
    }

    public void Die()
    {
        if (_isDead)
        {
            return;
        }

        // 사망 상태로 전환한다.
        _isDead = true;

        // 사망하면 입력, 이동, 상호작용을 모두 막는다.
        _isInputBlocked = true;
        _isMovementBlocked = true;
        _isInteractionBlocked = true;

        // 같은 GameObject 내부 또는 직접 구독자를 위한 이벤트를 발생시킨다.
        PlayerDied?.Invoke();

        // 게임 흐름 시스템에 플레이어 사망을 알린다.
        GameEventBus.RaisePlayerDied();
    }

    public void SetInputBlocked(bool isBlocked)
    {
        // 컷신, 결과 화면 등에서 전체 입력을 막을 수 있게 한다.
        _isInputBlocked = isBlocked;
    }

    public void SetMovementBlocked(bool isBlocked)
    {
        // 쇠지레 사용, 스턴 등에서 이동만 막을 수 있게 한다.
        _isMovementBlocked = isBlocked;
    }

    public void SetInteractionBlocked(bool isBlocked)
    {
        // 특정 상태에서 상호작용만 막을 수 있게 한다.
        _isInteractionBlocked = isBlocked;
    }

    public void ReviveForDebug()
    {
        // POC 테스트 중 재시작 없이 상태를 복구할 수 있게 한다.
        _isDead = false;
        _isInputBlocked = false;
        _isMovementBlocked = false;
        _isInteractionBlocked = false;
    }

    private void HandleRunEnded(GameResultType resultType)
    {
        // 탐색 종료 후에는 더 이상 조작하지 못하게 한다.
        _isInputBlocked = true;
        _isMovementBlocked = true;
        _isInteractionBlocked = true;
    }
}