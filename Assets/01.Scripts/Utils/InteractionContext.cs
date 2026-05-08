using UnityEngine;

public readonly struct InteractionContext
{
    public InteractionContext(GameObject interactor, PlayerCondition playerCondition, PlayerInventory playerInventory, PlayerOxygen playerOxygen)
    {
        Interactor = interactor;
        PlayerCondition = playerCondition;
        PlayerInventory = playerInventory;
        PlayerOxygen = playerOxygen;
    }

    public GameObject Interactor { get; }
    public PlayerCondition PlayerCondition { get; }
    public PlayerInventory PlayerInventory { get; }
    public PlayerOxygen PlayerOxygen { get; }
}