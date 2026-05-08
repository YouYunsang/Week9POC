using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData _itemData;
    [SerializeField] private int _amount = 1;
    [SerializeField] private int _interactionPriority = 10;
    [SerializeField] private bool _destroyOnPickup = true;

    public int InteractionPriority => _interactionPriority;

    private void Awake()
    {
        Collider2D itemCollider = GetComponent<Collider2D>();

        // 아이템은 물리적으로 막는 대상이 아니라 상호작용 감지 대상이다.
        itemCollider.isTrigger = true;
    }

    public bool CanInteract(InteractionContext context)
    {
        if (context.PlayerCondition == null || context.PlayerCondition.IsDead)
        {
            return false;
        }

        if (context.PlayerInventory == null)
        {
            return false;
        }

        // 아이템 데이터가 있어야 획득 상호작용이 가능하다.
        return _itemData != null && _amount > 0;
    }

    public void Interact(InteractionContext context)
    {
        if (!CanInteract(context))
        {
            return;
        }

        bool isAdded = context.PlayerInventory.TryAddItem(_itemData, _amount);

        if (!isAdded)
        {
            // 실패 이유는 PlayerInventory에서 Debug.Log로 출력한다.
            return;
        }

        if (_destroyOnPickup)
        {
            // 획득된 아이템은 맵에서 제거한다.
            Destroy(gameObject);
            return;
        }

        // 테스트용으로 파괴하지 않는 경우 중복 획득을 막기 위해 비활성화한다.
        gameObject.SetActive(false);
    }

    public string GetInteractionPrompt()
    {
        if (_itemData == null)
        {
            return "E: 획득";
        }

        return $"E: {_itemData.DisplayName} 획득";
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 획득 수량은 최소 1개로 제한한다.
        _amount = Mathf.Max(1, _amount);
    }
#endif
}