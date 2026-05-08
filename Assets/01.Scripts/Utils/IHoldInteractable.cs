using UnityEngine;

public interface IHoldInteractable : IInteractable
{
    bool CanBeginHold(InteractionContext context);

    void BeginHold(InteractionContext context);

    bool TickHold(InteractionContext context, float deltaTime);

    void CancelHold(InteractionContext context);
}