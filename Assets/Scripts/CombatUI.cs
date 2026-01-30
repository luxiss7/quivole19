using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche les barres de HP des joueurs et de l'ennemi
/// VERSION COMPATIBLE avec CombatManager_SIMPLE
/// </summary>
public class CombatUI : MonoBehaviour
{
    [Header("RÃ©fÃ©rences")]
    public CombatManager combat;
    
    [Header("Barres de HP")]
    public Slider enemyHP;
    public Slider[] playerHP; // 4 sliders pour les 4 joueurs

    void Update()
    {
        if (combat == null)
            return;

        // Pour l'instant, ce script est optionnel
        // Le systÃ¨me de combat fonctionne avec le Text de log uniquement
        
        // TODO: Si vous voulez des barres de HP, il faudra exposer
        // les listes de combattants dans CombatManager
    }
}