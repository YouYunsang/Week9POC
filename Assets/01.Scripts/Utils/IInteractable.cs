using UnityEngine;

public interface IInteractable
{
    //상호작용 가능한 오브젝트가 겹쳐 있을 경우 상호작용 우선순위 설정 가능
    int InteractionPriority { get; }

    bool CanInteract(InteractionContext context);

    void Interact(InteractionContext context);

    string GetInteractionPrompt();
}