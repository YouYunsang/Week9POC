using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemData",
    menuName = "ScriptableObjects/Item/Item Data")]
public sealed class ItemData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string _displayName;
    [SerializeField] private ItemCategory _itemCategory = ItemCategory.Resource;
    [SerializeField] private ToolType _toolType = ToolType.None;

    [Header("Inventory")]
    [SerializeField] private int _maxStackPerSlot = 1;
    [SerializeField] private float _weight = 1.0f;

    public string DisplayName => _displayName;
    public ItemCategory ItemCategory => _itemCategory;
    public ToolType ToolType => _toolType;
    public int MaxStackPerSlot => _maxStackPerSlot;
    public float Weight => _weight;
    public bool IsTool => _itemCategory == ItemCategory.Tool;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 아이템 이름이 비어 있으면 에셋 이름을 기본 표시명으로 사용한다.
        if (string.IsNullOrWhiteSpace(_displayName))
        {
            _displayName = name;
        }

        // 스택 수는 게임 규칙이 깨지지 않도록 최소값을 보장한다.
        _maxStackPerSlot = Mathf.Max(1, _maxStackPerSlot);

        // 무게는 0 이하가 되면 인벤토리 압박이 사라지므로 최소값을 보장한다.
        _weight = Mathf.Max(0.01f, _weight);

        // 도구가 아닌 아이템은 ToolType을 None으로 정리한다.
        if(_itemCategory != ItemCategory.Tool)
        {
            _toolType = ToolType.None;
        }
    }
#endif
}