using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCondition))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerOxygen))]
public sealed class PlayerInteractionDetector : MonoBehaviour
{
    private readonly List<IInteractable> _interactables = new List<IInteractable>();

    private PlayerCondition _condition;
    private PlayerInventory _inventory;
    private PlayerOxygen _oxygen;

    public bool HasInteractable => TryGetBestInteractable(out _);

    private void Reset()
    {
        // 같은 GameObject의 플레이어 상태 컴포넌트를 캐싱한다.
        _condition = GetComponent<PlayerCondition>();

        // 같은 GameObject의 인벤토리 컴포넌트를 캐싱한다.
        _inventory = GetComponent<PlayerInventory>();

        // 같은 GameObject의 산소 컴포넌트를 캐싱한다.
        _oxygen = GetComponent<PlayerOxygen>();
    }

    private void Awake()
    {
        // 같은 GameObject의 플레이어 상태 컴포넌트를 캐싱한다.
        _condition = GetComponent<PlayerCondition>();

        // 같은 GameObject의 인벤토리 컴포넌트를 캐싱한다.
        _inventory = GetComponent<PlayerInventory>();

        // 같은 GameObject의 산소 컴포넌트를 캐싱한다.
        _oxygen = GetComponent<PlayerOxygen>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Trigger에 들어온 오브젝트에서 상호작용 대상을 찾는다.
        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable == null)
        {
            interactable = other.GetComponentInParent<IInteractable>();
        }

        if (interactable == null)
        {
            return;
        }

        if (_interactables.Contains(interactable))
        {
            return;
        }

        _interactables.Add(interactable);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Trigger에서 나간 오브젝트의 상호작용 대상을 목록에서 제거한다.
        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable == null)
        {
            interactable = other.GetComponentInParent<IInteractable>();
        }

        if (interactable == null)
        {
            return;
        }

        _interactables.Remove(interactable);
    }

    public void TryInteract()
    {
        if (_condition == null || !_condition.CanInteract)
        {
            return;
        }

        if(!TryGetBestInteractable(out IInteractable interactable))
        {
            return;
        }

        InteractionContext context = CreateInteractionContext();

        if (!interactable.CanInteract(context))
        {
            return;
        }

        // 현재 가장 우선순위가 높은 대상과 상호작용한다.
        interactable.Interact(context);
    }

    public bool TryGetBestInteractable(out IInteractable bestInteractable)
    {
        bestInteractable = null;

        if(_interactables.Count <= 0)
        {
            return false;
        }

        int bestPriority = int.MinValue;
        InteractionContext context = CreateInteractionContext();

        for(int i = _interactables.Count - 1; i >=0; i--)
        {
            IInteractable interactable = _interactables[i];

            if(interactable == null)
            {
                _interactables.RemoveAt(i);
                continue;
            }

            if(!interactable.CanInteract(context))
                continue;

            if(interactable.InteractionPriority <= bestPriority)
                continue;

            bestInteractable = interactable;
            bestPriority = interactable.InteractionPriority;
        }

        return bestInteractable != null;
    }

    public InteractionContext CreateInteractionContext()
    {
        // 상호작용에 필요한 플레이어 관련 정보를 묶어서 전달한다.
        return new InteractionContext(gameObject, _condition, _inventory, _oxygen);
    }
}