using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (GameState.Instance != null)
        {
            GameState.Instance.ClearCombatData();
        }
    }
}
