using System;

public static class RFIDEventManager
{
    // lecteur, role
    public static Action<int, string> OnRFIDDetected;

    public static void TriggerRFID(int lecteur, string role)
    {
        OnRFIDDetected?.Invoke(lecteur, role);
    }
}