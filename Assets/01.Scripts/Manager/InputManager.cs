using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputManager : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _playerInputReader;
    [SerializeField] private CameraInputReader _cameraInputReader;

    public void OnMove(InputAction.CallbackContext context)
    {
        // Move 입력은 Vector2로 읽어서 PlayerInputReader에 전달한다.
        Vector2 moveInput = context.ReadValue<Vector2>();

        if (_playerInputReader == null)
        {
            return;
        }

        _playerInputReader.SetMoveInput(moveInput);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        // Sprint 입력은 버튼 상태로 읽어서 PlayerInputReader에 전달한다.
        bool isSprinting = context.ReadValueAsButton();

        if (_playerInputReader == null)
        {
            return;
        }

        _playerInputReader.SetSprintInput(isSprinting);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        if (_playerInputReader == null)
        {
            return;
        }

        // 상호작용은 performed 시점에 한 번만 전달한다.
        _playerInputReader.NotifyInteractInputStarted();
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        // Mouse Scroll 입력은 Vector2로 들어오며 y축만 줌 입력으로 사용한다.
        Vector2 scrollInput = context.ReadValue<Vector2>();

        if (_cameraInputReader == null)
        {
            return;
        }

        _cameraInputReader.SetZoomInput(scrollInput.y);
    }

    private void OnDisable()
    {
        // InputManager가 비활성화될 때 입력 상태가 남지 않도록 초기화한다.
        if (_playerInputReader != null)
        {
            _playerInputReader.ResetInput();
        }

        if (_cameraInputReader != null)
        {
            _cameraInputReader.ResetInput();
        }
    }
}