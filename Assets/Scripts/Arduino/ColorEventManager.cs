using System;

public static class ColorEventManager
{
    // color, value
    public static Action<string, int> OnColorDetected;

    public static void TriggerColor(string color, int value)
    {
        OnColorDetected?.Invoke(color, value);
    }
}
