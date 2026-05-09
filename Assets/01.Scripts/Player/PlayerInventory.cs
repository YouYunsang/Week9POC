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
    public float WeightRatio => _maxWeight <= 0.0f ? 0.0f : CurrentWeight / _maxWeight;
    public IReadOnlyList<ItemStack> Slots => _slots;

    private void Start()
    {
        AddStartItems();
    }

    public bool TryAddItem(ItemData itemData, int amount)
    {
        if (itemData == null)
        {
            Debug.LogWarning("Cannot add an empty ItemData.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("Item amount must be at least 1.");
            return false;
        }

        if (!CanFitItem(itemData, amount))
        {
            Debug.Log($"Inventory slots full: cannot add {itemData.DisplayName}. Slots {UsedSlotCount}/{_maxSlotCount}");
            return false;
        }

        AddItemInternal(itemData, amount);
        InventoryChanged?.Invoke();

        Debug.Log($"Picked up: {itemData.DisplayName} x{amount} / Slots {UsedSlotCount}/{_maxSlotCount}, Weight {CurrentWeight:0.0}/{_maxWeight:0.0}");

        return true;
    }

    public bool TryDropRandomNonToolItem(out ItemData droppedItemData)
    {
        droppedItemData = null;

        int droppableItemCount = 0;

        for (int i = 0; i < _slots.Count; i++)
        {
            ItemStack slot = _slots[i];

            if (slot.IsEmpty)
            {
                continue;
            }

            if (slot.ItemData.IsTool)
            {
                continue;
            }

            droppableItemCount += slot.Amount;
        }

        if (droppableItemCount <= 0)
        {
            Debug.Log("No non-tool item to drop.");
            return false;
        }

        int targetItemIndex = UnityEngine.Random.Range(0, droppableItemCount);
        int slotIndex = FindDroppableSlotIndex(targetItemIndex);

        if (slotIndex < 0)
        {
            return false;
        }

        ItemStack selectedSlot = _slots[slotIndex];
        droppedItemData = selectedSlot.ItemData;
        selectedSlot.RemoveAmount(1);

        if (selectedSlot.IsEmpty)
        {
            _slots.RemoveAt(slotIndex);
        }
        else
        {
            _slots[slotIndex] = selectedSlot;
        }

        InventoryChanged?.Invoke();

        Debug.Log($"Dropped: {droppedItemData.DisplayName} x1 / Slots {UsedSlotCount}/{_maxSlotCount}, Weight {CurrentWeight:0.0}/{_maxWeight:0.0}");

        return true;
    }

    private int FindDroppableSlotIndex(int targetItemIndex)
    {
        int currentItemIndex = 0;

        for (int i = 0; i < _slots.Count; i++)
        {
            ItemStack slot = _slots[i];

            if (slot.IsEmpty || slot.ItemData.IsTool)
            {
                continue;
            }

            currentItemIndex += slot.Amount;

            if (targetItemIndex < currentItemIndex)
            {
                return i;
            }
        }

        return -1;
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
        {
            return false;
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            ItemStack slot = _slots[i];

            if (slot.IsEmpty)
            {
                continue;
            }

            if (!slot.ItemData.IsTool)
            {
                continue;
            }

            if (slot.ItemData.ToolType == toolType)
            {
                return true;
            }
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

            totalAmount += _slots[i].Amount;
        }

        return totalAmount;
    }

    private void AddStartItems()
    {
        for (int i = 0; i < _startItems.Count; i++)
        {
            ItemStack startItem = _startItems[i];

            if (startItem.IsEmpty)
            {
                continue;
            }

            TryAddItem(startItem.ItemData, startItem.Amount);
        }
    }

    private bool CanFitItem(ItemData itemData, int amount)
    {
        int remainingAmount = amount;

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

        return requiredNewSlotCount <= emptySlotCount;
    }

    private void AddItemInternal(ItemData itemData, int amount)
    {
        int remainingAmount = amount;

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
            totalWeight += _slots[i].TotalWeight;
        }

        return totalWeight;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _maxSlotCount = Mathf.Max(1, _maxSlotCount);
        _maxWeight = Mathf.Max(0.01f, _maxWeight);
    }
#endif
}
