using UnityEngine;

public class CombatZoneManager : MonoBehaviour
{
    public static CombatZoneManager Instance;

    [Header("Positions Combat (fixes dans la scène)")]
    public Transform[] combatPositions = new Transform[4];

    private Vector3[] donjonPositions = new Vector3[4];
    private bool combatEnCours = false;

    void Awake()
    {
        Instance = this;
    }

    // 🔹 appelé en permanence tant qu'on n'est PAS en combat
    public void SauvegarderPositionsDonjon()
    {
        if (combatEnCours) return;

        Player[] joueurs = FindObjectsOfType<Player>();
        for (int i = 0; i < Mathf.Min(4, joueurs.Length); i++)
        {
            donjonPositions[i] = joueurs[i].transform.position;
        }
    }

    // 🔥 appelé par EnemyTrigger
    public void LancerCombat(EnemyData enemyData, System.Action onCombatFinished)
    {
        combatEnCours = true;

        Player[] joueurs = FindObjectsOfType<Player>();

        // TP vers zone combat
        for (int i = 0; i < Mathf.Min(4, joueurs.Length); i++)
        {
            var pm = joueurs[i].GetComponent<PlayerMovement>();
            pm.peutBouger = false;
            pm.transform.position = combatPositions[i].position;
        }

        // Préparer GameState
        GameState.Instance.party.Clear();
        foreach (var j in joueurs)
            GameState.Instance.party.Add(j.CreerPlayerData());

        GameState.Instance.enemyData = enemyData;

        // Démarrer le combat
        FindObjectOfType<CombatManager>()
            ?.StartCombat(() =>
            {
                FinCombat();
                onCombatFinished?.Invoke();
            });
    }

    void FinCombat()
    {
        Player[] joueurs = FindObjectsOfType<Player>();

        // Retour donjon
        for (int i = 0; i < Mathf.Min(4, joueurs.Length); i++)
        {
            joueurs[i].transform.position = donjonPositions[i];
            joueurs[i].GetComponent<PlayerMovement>().peutBouger = true;
        }

        combatEnCours = false;
    }
}
