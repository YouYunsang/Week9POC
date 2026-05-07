using System;
using UnityEngine;

public sealed class CameraInputReader : MonoBehaviour
{
    public event Action<float> ZoomInputChanged;

    public float ZoomInput { get; private set; }

    public void SetZoomInput(float zoomInput)
    {
        // InputManager에서 전달받은 마우스 휠 입력을 저장한다.
        ZoomInput = zoomInput;

        // PlayerCameraZoom에 줌 입력 변경을 알린다.
        ZoomInputChanged?.Invoke(ZoomInput);
    }

    public void ResetInput()
    {
        // 줌 입력은 순간 입력이므로 비활성화 시 0으로 초기화한다.
        ZoomInput = 0.0f;

        ZoomInputChanged?.Invoke(ZoomInput);
    }

    private void OnDisable()
    {
        // CameraRig 비활성화 시 입력 상태를 초기화한다.
        ResetInput();
    }
}