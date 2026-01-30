using UnityEngine;

/// <summary>
/// Gère les zones de combat et la téléportation des joueurs
/// VERSION CORRIGÉE pour CombatManager_ULTRA_FINAL
/// </summary>
public class CombatZoneManager : MonoBehaviour
{
    public static CombatZoneManager Instance;

    [Header("Positions Combat (fixes dans la scène)")]
    public Transform[] combatPositions = new Transform[4];

    private Vector3[] donjonPositions = new Vector3[4];
    private bool combatEnCours = false;
    private GameObject enemyGameObject; // Pour passer au CombatManager

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Sauvegarde les positions actuelles des joueurs dans le donjon
    /// Appelé en permanence tant qu'on n'est PAS en combat
    /// </summary>
    public void SauvegarderPositionsDonjon()
    {
        if (combatEnCours) 
            return;

        Player[] joueurs = FindObjectsOfType<Player>();
        for (int i = 0; i < Mathf.Min(4, joueurs.Length); i++)
        {
            donjonPositions[i] = joueurs[i].transform.position;
        }
    }

    /// <summary>
    /// Lance un combat - appelé par EnemyTrigger
    /// </summary>
    public void LancerCombat(GameObject enemyGO, EnemyData enemyData, System.Action onCombatFinished = null)
    {
        if (combatEnCours)
        {
            Debug.LogWarning("[CombatZoneManager] Combat déjà en cours !");
            return;
        }

        if (enemyData == null)
        {
            Debug.LogError("[CombatZoneManager] EnemyData est null !");
            return;
        }

        combatEnCours = true;
        enemyGameObject = enemyGO;

        Debug.Log($"[CombatZoneManager] Lancement combat contre {enemyData.nom}");

        Player[] joueurs = FindObjectsOfType<Player>();

        // Sauvegarder les positions actuelles si pas déjà fait
        for (int i = 0; i < Mathf.Min(4, joueurs.Length); i++)
        {
            donjonPositions[i] = joueurs[i].transform.position;
        }

        // ✅ NOUVEAU : Utiliser directement CombatManager.Instance.StartCombat()
        // Le CombatManager gère maintenant la téléportation lui-même
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.StartCombat(enemyGO, enemyData);
        }
        else
        {
            Debug.LogError("[CombatZoneManager] CombatManager.Instance est null !");
            combatEnCours = false;
        }

        // Note : La fin du combat est gérée par CombatManager maintenant
        // Si vous voulez un callback, il faudra l'ajouter au CombatManager
    }

    /// <summary>
    /// Fin du combat - réinitialise l'état
    /// (Cette méthode peut être appelée par CombatManager si vous ajoutez un système de callback)
    /// </summary>
    public void FinCombat()
    {
        Debug.Log("[CombatZoneManager] Fin du combat");
        combatEnCours = false;
        enemyGameObject = null;
    }

    /// <summary>
    /// MÉTHODE ALTERNATIVE : Si vous voulez gérer la téléportation ici
    /// (Mais ce n'est pas recommandé car CombatManager le fait déjà)
    /// </summary>
    public void TeleporterVersZoneCombat()
    {
        Player[] joueurs = FindObjectsOfType<Player>();

        for (int i = 0; i < Mathf.Min(4, joueurs.Length, combatPositions.Length); i++)
        {
            if (combatPositions[i] == null)
            {
                Debug.LogError($"[CombatZoneManager] Position combat {i} est null !");
                continue;
            }

            var pm = joueurs[i].GetComponent<PlayerMovement>();
            if (pm != null)
                pm.peutBouger = false;
            
            joueurs[i].transform.position = combatPositions[i].position;
        }
    }

    /// <summary>
    /// Retour au donjon après le combat
    /// (Cette méthode peut être appelée par CombatManager si vous ajoutez un système de callback)
    /// </summary>
    public void RetourDonjon()
    {
        Player[] joueurs = FindObjectsOfType<Player>();

        for (int i = 0; i < Mathf.Min(4, joueurs.Length); i++)
        {
            joueurs[i].transform.position = donjonPositions[i];
            
            var pm = joueurs[i].GetComponent<PlayerMovement>();
            if (pm != null)
                pm.peutBouger = true;
        }

        combatEnCours = false;
    }
}