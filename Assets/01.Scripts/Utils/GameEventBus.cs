using System;

public static class GameEventBus
{
    public static event Action ReturnToBaseRequested;
    public static event Action PlayerDied;
    public static event Action<GameResultType> RunEnded;

    public static void RaiseReturnToBaseRequested()
    {
        ReturnToBaseRequested?.Invoke();
    }

    public static void RaisePlayerDied()
    {
        PlayerDied?.Invoke();
    }

    public static void RaiseRunEnded(GameResultType resultType)
    {
        RunEnded?.Invoke(resultType);
    }
}