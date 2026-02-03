using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Déclenche un combat quand le joueur touche l'ennemi
/// VERSION CORRIGÉE pour fonctionner avec CombatManager_ULTRA_FINAL
/// </summary>
public class EnemyTrigger : MonoBehaviour
{
    [Header("Configuration")]
    public EnemyData enemyData;
    
    private bool combatDeclenche = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifier si c'est un joueur
        Player player = other.GetComponent<Player>();
        if (player == null)
            return;

        // Éviter de déclencher plusieurs fois
        if (combatDeclenche)
            return;

        // Vérifier que l'EnemyData existe
        if (enemyData == null)
        {
            Debug.LogError($"[EnemyTrigger] {gameObject.name} n'a pas d'EnemyData assigné !");
            return;
        }

        if (enemyData.nom == "Dragon" && GameState.Instance.dragonEggRecupere)
        {
            SceneManager.LoadScene("FriendlyCredits");
            return;
        }

        combatDeclenche = true;

        Debug.Log($"[EnemyTrigger] Collision avec {enemyData.nom} !");
        
        // ✅ OPTION 1 : Utiliser directement CombatManager (RECOMMANDÉ)
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.StartCombat(gameObject, enemyData);
        }
        else
        {
            Debug.LogError("[EnemyTrigger] CombatManager.Instance n'existe pas !");
        }

        // ✅ OPTION 2 : Utiliser CombatZoneManager (si vous voulez garder cette architecture)
        // Décommentez ces lignes et commentez l'option 1 si vous préférez
        /*
        if (CombatZoneManager.Instance != null)
        {
            CombatZoneManager.Instance.LancerCombat(gameObject, enemyData, () => {
                // Callback quand le combat est fini (optionnel)
                Debug.Log($"[EnemyTrigger] Combat contre {enemyData.nom} terminé");
            });
        }
        else
        {
            Debug.LogError("[EnemyTrigger] CombatZoneManager.Instance n'existe pas !");
        }
        */
    }

    // Réinitialiser si on veut retester (pour debug)
    void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && !CombatManager.Instance.combatEnCours)
        {
            combatDeclenche = false;
        }
    }

    // Pour visualiser la zone de détection dans l'éditeur
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}