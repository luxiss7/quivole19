using UnityEngine;
using System;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public enum CombatState { Start, PlayerTurn, EnemyTurn, Victory, Defeat }
    public CombatState state;

    public List<PlayerCombatant> joueurs = new();
    public EnemyCombatant ennemi;

    private int indexJoueur = 0;
    private Action onCombatTermine;

    [Header("Combat Visuals")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Transform[] playerSlots;
    public Transform enemySlot;

    // ----------------- Nouveau -----------------
    public void StartCombat(Action finCombatCallback)
    {
        onCombatTermine = finCombatCallback;

        if (GameState.Instance == null ||
            GameState.Instance.enemyData == null ||
            GameState.Instance.party.Count == 0)
        {
            Debug.LogWarning("[CombatManager] Pas de données valides pour le combat !");
            onCombatTermine?.Invoke();
            return;
        }

        InitialiserCombat();
    }

    void InitialiserCombat()
    {
        joueurs.Clear();

        foreach (var p in GameState.Instance.party)
            joueurs.Add(new PlayerCombatant(p));

        ennemi = new EnemyCombatant(GameState.Instance.enemyData);

        SpawnVisuels();

        state = CombatState.PlayerTurn;
        indexJoueur = 0;

        Debug.Log("[CombatManager] Combat contre " + ennemi.data.nom);
    }

    void SpawnVisuels()
    {
        // JOUEURS
        for (int i = 0; i < joueurs.Count; i++)
        {
            GameObject go = Instantiate(playerPrefab, playerSlots[i].position, Quaternion.identity);
            go.GetComponent<PlayerCombatView>().Init(joueurs[i]);
        }

        // ENNEMI
        GameObject enemyGO = Instantiate(enemyPrefab, enemySlot.position, Quaternion.identity);
        enemyGO.GetComponent<EnemyCombatView>().Init(ennemi);
    }

    // ================= TOUR JOUEUR =================
    public void PlayerAttack()
    {
        if (state != CombatState.PlayerTurn) return;

        PlayerCombatant j = joueurs[indexJoueur];

        int degats = j.data.melee + j.data.arme1.degats;
        ennemi.hpActuels -= degats;

        Debug.Log($"Joueur {indexJoueur} attaque : {degats}");

        if (ennemi.hpActuels <= 0)
        {
            state = CombatState.Victory;
            FinCombat();
            return;
        }

        ProchainJoueur();
    }

    void ProchainJoueur()
    {
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
            if (!j.estVivant) continue;

            int degats = ennemi.data.degats;
            j.hpActuels -= degats;

            if (j.hpActuels <= 0)
                j.estVivant = false;
        }

        if (TousLesJoueursMorts())
        {
            state = CombatState.Defeat;
            FinCombat();
        }
        else
        {
            state = CombatState.PlayerTurn;
        }
    }

    bool TousLesJoueursMorts()
    {
        foreach (var j in joueurs)
            if (j.estVivant) return false;
        return true;
    }

    void FinCombat()
    {
        Debug.Log("[CombatManager] Fin du combat : " + state);

        GameState.Instance.ClearCombatData();

        // Callback pour restaurer les joueurs et reprendre la map
        onCombatTermine?.Invoke();
    }
}
