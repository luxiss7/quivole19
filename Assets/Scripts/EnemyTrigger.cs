using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    public EnemyData enemyData;

    [Header("Positions combat")]
    public Transform CombatPositionJoueur1;
    public Transform CombatPositionJoueur2;
    public Transform CombatPositionJoueur3;
    public Transform CombatPositionJoueur4;

    private bool combatDeclenche = false;

    private Vector3[] donjonPositions = new Vector3[4];

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (combatDeclenche) return;

        Player p = other.GetComponent<Player>();
        if (p == null) return;

        combatDeclenche = true;

        // Sauvegarde positions donjon
        Player[] joueurs = FindObjectsOfType<Player>();
        for (int i = 0; i < Mathf.Min(4, joueurs.Length); i++)
            donjonPositions[i] = joueurs[i].transform.position;

        // Déplacement vers positions de combat
        for (int i = 0; i < Mathf.Min(4, joueurs.Length); i++)
        {
            joueurs[i].GetComponent<PlayerMovement>().peutBouger = false;
            joueurs[i].transform.position = GetCombatPosition(i);
        }

        // Préparer GameState pour le combat
        GameState.Instance.joueurs.Clear();
        GameState.Instance.party.Clear();

        foreach (var j in joueurs)
        {
            PlayerData data = j.CreerPlayerData();
            data.position = j.GetComponent<PlayerMovement>().position;
            GameState.Instance.joueurs.Add(data);
            GameState.Instance.party.Add(data);
        }

        GameState.Instance.enemyData = enemyData;

        // Lancer le combat dans la même scène
        FindObjectOfType<CombatManager>()?.StartCombat(FinCombat);
    }

    Vector3 GetCombatPosition(int index)
    {
        switch (index)
        {
            case 0: return CombatPositionJoueur1.position;
            case 1: return CombatPositionJoueur2.position;
            case 2: return CombatPositionJoueur3.position;
            case 3: return CombatPositionJoueur4.position;
            default: return CombatPositionJoueur1.position;
        }
    }

    void FinCombat()
    {
        // Restaurer positions donjon
        Player[] joueurs = FindObjectsOfType<Player>();
        for (int i = 0; i < Mathf.Min(4, joueurs.Length); i++)
        {
            joueurs[i].transform.position = donjonPositions[i];
            joueurs[i].GetComponent<PlayerMovement>().peutBouger = true;
        }

        // Nettoyage
        combatDeclenche = false;

        Debug.Log("[CombatTrigger] Fin du combat, joueurs restaurés");
    }
}
