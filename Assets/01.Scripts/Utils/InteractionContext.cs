using UnityEngine;

public readonly struct InteractionContext
{
    public InteractionContext(
        GameObject interactor,
        PlayerCondition playerCondition)
    {
        Interactor = interactor;
        PlayerCondition = playerCondition;
    }

    public GameObject Interactor { get; }
    public PlayerCondition PlayerCondition { get; }
}