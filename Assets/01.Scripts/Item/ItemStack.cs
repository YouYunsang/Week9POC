using System;
using UnityEngine;

[Serializable]
public struct ItemStack
{
    [SerializeField] private ItemData _itemData;
    [SerializeField] private int _amount;

    public ItemData ItemData => _itemData;
    public int Amount => _amount;
    public bool IsEmpty => _itemData == null || _amount <= 0;
    public float TotalWeight => IsEmpty ? 0.0f : _itemData.Weight * _amount;

    public ItemStack(ItemData itemData, int amount)
    {
        // 아이템 데이터와 수량을 하나의 슬롯 단위로 묶는다.
        _itemData = itemData;
        _amount = Mathf.Max(0, amount);
    }

    public bool CanStackWith(ItemData itemData)
    {
        if (IsEmpty)
        {
            return false;
        }

        // 같은 ItemData를 가진 아이템만 같은 슬롯에 스택할 수 있다.
        return _itemData == itemData;
    }

    public int GetRemainingStackCapacity()
    {
        if (IsEmpty)
        {
            return 0;
        }

        // 현재 슬롯에 더 들어갈 수 있는 개수를 계산한다.
        return Mathf.Max(0, _itemData.MaxStackPerSlot - _amount);
    }

    public void AddAmount(int amount)
    {
        if (IsEmpty)
        {
            return;
        }

        // 슬롯 최대 스택 수를 넘지 않도록 수량을 더한다.
        _amount = Mathf.Clamp(_amount + amount, 0, _itemData.MaxStackPerSlot);
    }
}