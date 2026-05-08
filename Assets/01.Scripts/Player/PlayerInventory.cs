using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerInventory : MonoBehaviour
{
    private const int DEFAULT_SLOT_COUNT = 4;

    [Header("Limit")]
    [SerializeField] private int _maxSlotCount = DEFAULT_SLOT_COUNT;
    [SerializeField] private float _maxWeight = 6.0f;

    [Header("Start Items")]
    [SerializeField] private List<ItemStack> _startItems = new List<ItemStack>();

    private readonly List<ItemStack> _slots = new List<ItemStack>();

    public event Action InventoryChanged;

    public int MaxSlotCount => _maxSlotCount;
    public float MaxWeight => _maxWeight;
    public int UsedSlotCount => _slots.Count;
    public float CurrentWeight => CalculateCurrentWeight();
    public IReadOnlyList<ItemStack> Slots => _slots;

    private void Start()
    {
        AddStartItems();
    }

    public bool TryAddItem(ItemData itemData, int amount)
    {
        if (itemData == null)
        {
            Debug.LogWarning("획득하려는 ItemData가 비어 있습니다.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("획득 수량은 1 이상이어야 합니다.");
            return false;
        }

        float addedWeight = itemData.Weight * amount;

        if (CurrentWeight + addedWeight > _maxWeight)
        {
            Debug.Log($"무게 초과: {itemData.DisplayName}을(를) 획득할 수 없습니다. 현재 무게 {CurrentWeight:0.0}/{_maxWeight:0.0}");
            return false;
        }

        if (!CanFitItem(itemData, amount))
        {
            Debug.Log($"인벤토리 공간 부족: {itemData.DisplayName}을(를) 획득할 수 없습니다. 슬롯 {UsedSlotCount}/{_maxSlotCount}");
            return false;
        }

        // 실제 아이템 추가를 수행한다.
        AddItemInternal(itemData, amount);

        // 인벤토리 변경을 외부 시스템에 알린다.
        InventoryChanged?.Invoke();

        Debug.Log($"획득: {itemData.DisplayName} x{amount} / 슬롯 {UsedSlotCount}/{_maxSlotCount}, 무게 {CurrentWeight:0.0}/{_maxWeight:0.0}");

        return true;
    }

    public bool HasItem(ItemData itemData)
    {
        if (itemData == null)
        {
            return false;
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].ItemData == itemData && !_slots[i].IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasTool(ToolType toolType)
    {
        if (toolType == ToolType.None)
            return false;

        for(int i = 0; i < _slots.Count; i++)
        {
            ItemStack slot = _slots[i];

            if (slot.IsEmpty)
                continue;

            if (!slot.ItemData.IsTool)
                continue;

            if(slot.ItemData.ToolType == toolType) return true;
        }

        return false;
    }

    public int GetItemAmount(ItemData itemData)
    {
        if (itemData == null)
        {
            return 0;
        }

        int totalAmount = 0;

        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].ItemData != itemData)
            {
                continue;
            }

            // 같은 아이템의 전체 보유 수량을 누적한다.
            totalAmount += _slots[i].Amount;
        }

        return totalAmount;
    }

    private void AddStartItems()
    {
        for(int i = 0; i < _startItems.Count; i++)
        {
            ItemStack startItem = _startItems[i];

            if (startItem.IsEmpty)
                continue;

            TryAddItem(startItem.ItemData, startItem.Amount);
        }
    }

    private bool CanFitItem(ItemData itemData, int amount)
    {
        int remainingAmount = amount;

        // 기존 슬롯에 스택 가능한 양을 먼저 계산한다.
        for (int i = 0; i < _slots.Count; i++)
        {
            ItemStack slot = _slots[i];

            if (!slot.CanStackWith(itemData))
            {
                continue;
            }

            remainingAmount -= slot.GetRemainingStackCapacity();

            if (remainingAmount <= 0)
            {
                return true;
            }
        }

        int emptySlotCount = _maxSlotCount - _slots.Count;

        if (emptySlotCount <= 0)
        {
            return false;
        }

        int requiredNewSlotCount = Mathf.CeilToInt(
            remainingAmount / (float)itemData.MaxStackPerSlot);

        // 필요한 새 슬롯 수가 빈 슬롯 수 이하인지 확인한다.
        return requiredNewSlotCount <= emptySlotCount;
    }

    private void AddItemInternal(ItemData itemData, int amount)
    {
        int remainingAmount = amount;

        // 기존 슬롯에 먼저 채운다.
        for (int i = 0; i < _slots.Count; i++)
        {
            if (remainingAmount <= 0)
            {
                return;
            }

            ItemStack slot = _slots[i];

            if (!slot.CanStackWith(itemData))
            {
                continue;
            }

            int addAmount = Mathf.Min(
                remainingAmount,
                slot.GetRemainingStackCapacity());

            slot.AddAmount(addAmount);
            _slots[i] = slot;

            remainingAmount -= addAmount;
        }

        // 남은 수량은 새 슬롯에 넣는다.
        while (remainingAmount > 0)
        {
            int addAmount = Mathf.Min(remainingAmount, itemData.MaxStackPerSlot);

            _slots.Add(new ItemStack(itemData, addAmount));

            remainingAmount -= addAmount;
        }
    }

    private float CalculateCurrentWeight()
    {
        float totalWeight = 0.0f;

        for (int i = 0; i < _slots.Count; i++)
        {
            // 각 슬롯의 총 무게를 합산한다.
            totalWeight += _slots[i].TotalWeight;
        }

        return totalWeight;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // POC 기본 규칙은 3칸이지만, 테스트 편의를 위해 최소값만 보장한다.
        _maxSlotCount = Mathf.Max(1, _maxSlotCount);

        // 무게 제한은 0 이하가 되면 모든 아이템 획득이 불가능해지므로 최소값을 보장한다.
        _maxWeight = Mathf.Max(0.01f, _maxWeight);
    }
#endif
}