using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    public enum CombatState { PlayerTurn, EnemyTurn, Victory, Defeat }
    public CombatState state;

    public List<Player> joueurs = new();
    public EnemyData enemyData;
    public GameObject enemyGO; // ennemi dans le donjon

    int indexJoueur;

    void Awake()
    {
        Instance = this;
    }

    // 🔥 LANCEMENT COMBAT
    public void StartCombat(GameObject enemyGO, EnemyData data)
    {
        this.enemyGO = enemyGO;
        this.enemyData = data;

        joueurs.Clear();
        indexJoueur = 0;

        joueurs.AddRange(FindObjectsOfType<Player>());

        // Geler déplacements
        foreach (var j in joueurs)
            j.GetComponent<PlayerMovement>().peutBouger = false;

        state = CombatState.PlayerTurn;

        Debug.Log("[Combat] Combat lancé contre " + enemyData.nom);
    }

    // ================= TOUR JOUEUR =================
    public void PlayerAttack()
    {
        if (state != CombatState.PlayerTurn) return;

        Player j = joueurs[indexJoueur];

        int degats = j.melee + j.arme1.degats;
        enemyData.pointsDeVie -= degats;

        Debug.Log($"[Combat] {j.name} attaque : {degats}");

        if (enemyData.pointsDeVie <= 0)
        {
            state = CombatState.Victory;
            FinCombat();
            return;
        }

        indexJoueur++;
        if (indexJoueur >= joueurs.Count)
        {
            indexJoueur = 0;
            state = CombatState.EnemyTurn;
            Invoke(nameof(EnemyAttack), 1f);
        }
    }

    // ================= TOUR ENNEMI =================
    void EnemyAttack()
    {
        foreach (var j in joueurs)
        {
            j.pointsDeVie -= enemyData.degats;
            Debug.Log("[Combat] Ennemi attaque");
        }

        state = CombatState.PlayerTurn;
    }

    // ================= FIN =================
    void FinCombat()
    {
        foreach (var j in joueurs)
            j.GetComponent<PlayerMovement>().peutBouger = true;

        if (state == CombatState.Victory && enemyGO != null)
            Destroy(enemyGO);

        Debug.Log("[Combat] Fin du combat : " + state);
    }
}
