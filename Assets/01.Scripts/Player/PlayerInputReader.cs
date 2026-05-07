using System;
using UnityEngine;

public sealed class PlayerInputReader : MonoBehaviour
{
    public event Action<Vector2> MoveInputChanged;
    public event Action<bool> SprintInputChanged;
    public event Action InteractInputStarted;

    public Vector2 MoveInput { get; private set; }
    public bool IsSprinting { get; private set; }

    public void SetMoveInput(Vector2 moveInput)
    {
        // InputManager에서 전달받은 이동 입력을 저장한다.
        MoveInput = moveInput;

        // PlayerController에 이동 입력 변경을 알린다.
        MoveInputChanged?.Invoke(MoveInput);
    }

    public void SetSprintInput(bool isSprinting)
    {
        // InputManager에서 전달받은 빠른 헤엄 입력을 저장한다.
        IsSprinting = isSprinting;

        // PlayerController에 빠른 헤엄 상태 변경을 알린다.
        SprintInputChanged?.Invoke(IsSprinting);
    }

    public void NotifyInteractInputStarted()
    {
        // 상호작용 입력이 시작되었음을 알린다.
        InteractInputStarted?.Invoke();
    }

    public void ResetInput()
    {
        // 입력 상태가 남아 플레이어가 계속 움직이는 문제를 방지한다.
        MoveInput = Vector2.zero;
        IsSprinting = false;

        MoveInputChanged?.Invoke(MoveInput);
        SprintInputChanged?.Invoke(IsSprinting);
    }

    private void OnDisable()
    {
        // Player 오브젝트가 비활성화될 때 입력 상태를 초기화한다.
        ResetInput();
    }
}