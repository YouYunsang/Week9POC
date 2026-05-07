using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class ReturnPoint : MonoBehaviour, IInteractable
{
    [SerializeField] private int _interactionPriority = 100;
    [SerializeField] private string _interactionPrompt = "E: 거점으로 복귀";

    public int InteractionPriority => _interactionPriority;

    private void Awake()
    {
        Collider2D returnPointCollider = GetComponent<Collider2D>();

        // 복귀 가능 영역은 물리 충돌이 아니라 감지 영역이어야 한다.
        returnPointCollider.isTrigger = true;
    }

    public bool CanInteract(InteractionContext context)
    {
        if (context.PlayerCondition == null)
        {
            return false;
        }

        // 죽은 상태에서는 복귀할 수 없다.
        return !context.PlayerCondition.IsDead;
    }

    public void Interact(InteractionContext context)
    {
        if (!CanInteract(context))
        {
            return;
        }

        // 복귀 요청을 게임 흐름 시스템에 알린다.
        GameEventBus.RaiseReturnToBaseRequested();

        Debug.Log("복귀 가능 지점에서 거점 복귀를 요청했습니다.");
    }

    public string GetInteractionPrompt()
    {
        return _interactionPrompt;
    }
}