using UnityEngine;

public readonly struct NoiseEventData
{
    public NoiseEventData(
        Vector2 position,
        float radius,
        float intensity,
        GameObject source)
    {
        Position = position;
        Radius = radius;
        Intensity = intensity;
        Source = source;
    }

    public Vector2 Position { get; }
    public float Radius { get; }
    public float Intensity { get; }
    public GameObject Source { get; }
}