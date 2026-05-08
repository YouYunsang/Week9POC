public readonly struct OxygenWarningViewState
{
    public OxygenWarningViewState(
        OxygenWarningStage stage,
        float intensity,
        float vignetteAlpha,
        float overlayAlpha,
        bool shouldShowWarningText,
        string warningText)
    {
        Stage = stage;
        Intensity = intensity;
        VignetteAlpha = vignetteAlpha;
        OverlayAlpha = overlayAlpha;
        ShouldShowWarningText = shouldShowWarningText;
        WarningText = warningText;
    }

    public OxygenWarningStage Stage { get; }
    public float Intensity { get; }
    public float VignetteAlpha { get; }
    public float OverlayAlpha { get; }
    public bool ShouldShowWarningText { get; }
    public string WarningText { get; }
}