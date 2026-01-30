using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// VERSION FINALE - SYSTÈME KO COMPLET (SANS SYSTÈME D'ARMES)
/// ✅ Les joueurs KO ne peuvent plus attaquer pendant le combat
/// ✅ Les sprites des joueurs KO disparaissent PENDANT le combat
/// ✅ Les sprites réapparaissent APRÈS le combat (joueur immobilisé mais visible)
/// ✅ Skip automatique du tour des joueurs KO
/// ✅ Réanimation avec HP pleins au prochain combat
/// ✅ Les joueurs KO peuvent lancer les dés (c'est juste le mouvement qui est bloqué)
/// </summary>
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Positions de combat")]
    public Transform[] playerCombatPositions;
    public Transform enemyPosition;

    [Header("UI Combat")]
    public GameObject combatUI;
    public UnityEngine.UI.Text logText;

    [Header("Prefabs (Optionnel)")]
    public GameObject enemyVisualPrefab;

    // État du combat
    private List<PlayerCombatant> joueurs = new List<PlayerCombatant>();
    private EnemyCombatant ennemi;
    private int indexJoueur = 0;
    public bool combatEnCours = false;
    private bool actionEnCours = false;
    
    // Sauvegarde
    private Vector3[] positionsDonjon = new Vector3[4];
    private GameObject ennemyGameObject;
    private GameObject enemyVisualInstance;
    
    // ✅ Références aux GameObjects des joueurs dans la zone de combat
    private Player[] playersInCombat;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (combatUI != null)
            combatUI.SetActive(false);
        else
            Debug.LogError("[Combat] combatUI non assigné !");

        if (logText != null)
            logText.text = "";
        else
            Debug.LogWarning("[Combat] logText non assigné !");
    }

    // ═══════════════════════════════════════════════════════════
    // DÉMARRAGE DU COMBAT
    // ═══════════════════════════════════════════════════════════
    public void StartCombat(GameObject enemyGO, EnemyData enemyData)
    {
        if (combatEnCours)
        {
            Debug.LogWarning("[Combat] Combat déjà en cours !");
            return;
        }

        if (enemyData == null)
        {
            Debug.LogError("[Combat] EnemyData null !");
            return;
        }

        combatEnCours = true;
        actionEnCours = false;
        ennemyGameObject = enemyGO;
        
        if (logText != null)
            logText.text = "";

        Log("=== COMBAT COMMENCE ===");
        Log($"Vous affrontez : {enemyData.nom}");

        // Trouver joueurs
        playersInCombat = FindObjectsOfType<Player>();
        
        if (playersInCombat.Length == 0)
        {
            Debug.LogError("[Combat] Aucun joueur !");
            combatEnCours = false;
            return;
        }

        // ✅ RÉANIMER les joueurs KO avec HP pleins
        foreach (var p in playersInCombat)
        {
            p.Reanimer();
            
            // ✅ S'assurer que le sprite est visible
            SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = true;
        }

        // Sauvegarder positions
        for (int i = 0; i < playersInCombat.Length && i < positionsDonjon.Length; i++)
        {
            positionsDonjon[i] = playersInCombat[i].transform.position;
        }

        // Créer combattants
        joueurs.Clear();
        foreach (var p in playersInCombat)
        {
            if (p.classeData == null)
            {
                Debug.LogError($"[Combat] {p.name} sans ClasseData !");
                continue;
            }

            PlayerData playerData = p.CreerPlayerData();
            if (playerData != null && playerData.classeData != null)
            {
                PlayerCombatant combattant = new PlayerCombatant(playerData);
                joueurs.Add(combattant);
                Debug.Log($"[Combat] Joueur: {playerData.classeData.nomClasse} ({playerData.pointsDeVie} HP)");
            }
            else
            {
                Debug.LogError($"[Combat] PlayerData invalide pour {p.name}");
            }
        }

        if (joueurs.Count == 0)
        {
            Debug.LogError("[Combat] Aucun joueur valide !");
            combatEnCours = false;
            return;
        }

        // Créer ennemi
        ennemi = new EnemyCombatant(enemyData);
        Debug.Log($"[Combat] Ennemi: {ennemi.data.nom} ({ennemi.hpActuels} HP)");

        // Téléporter joueurs
        if (playerCombatPositions == null || playerCombatPositions.Length == 0)
        {
            Debug.LogError("[Combat] Positions non configurées !");
            combatEnCours = false;
            return;
        }

        for (int i = 0; i < playersInCombat.Length && i < playerCombatPositions.Length; i++)
        {
            if (playerCombatPositions[i] == null)
            {
                Debug.LogError($"[Combat] Position {i} null !");
                continue;
            }

            playersInCombat[i].transform.position = playerCombatPositions[i].position;
            
            PlayerMovement pm = playersInCombat[i].GetComponent<PlayerMovement>();
            if (pm != null)
                pm.peutBouger = false;
        }

        // Créer sprite ennemi
        CreerSpriteEnnemi(enemyData);

        // Activer UI
        if (combatUI != null)
            combatUI.SetActive(true);

        // Premier tour
        indexJoueur = 0;
        AfficherTourJoueur();
    }

    void CreerSpriteEnnemi(EnemyData enemyData)
    {
        if (enemyVisualInstance != null)
            Destroy(enemyVisualInstance);

        if (enemyPosition == null)
        {
            Debug.LogError("[Combat] enemyPosition null !");
            return;
        }

        if (enemyVisualPrefab != null)
        {
            enemyVisualInstance = Instantiate(enemyVisualPrefab, enemyPosition.position, Quaternion.identity);
            
            SpriteRenderer sr = enemyVisualInstance.GetComponent<SpriteRenderer>();
            if (sr != null && enemyData.sprite != null)
            {
                sr.sprite = enemyData.sprite;
                Debug.Log($"[Combat] Sprite créé (prefab)");
            }
        }
        else
        {
            if (enemyData.sprite == null)
            {
                Debug.LogError($"[Combat] {enemyData.nom} sans sprite !");
                return;
            }

            enemyVisualInstance = new GameObject($"Enemy_{enemyData.nom}");
            enemyVisualInstance.transform.position = enemyPosition.position;
            
            SpriteRenderer sr = enemyVisualInstance.AddComponent<SpriteRenderer>();
            sr.sprite = enemyData.sprite;
            sr.sortingOrder = 10;
            
            Debug.Log($"[Combat] Sprite créé à {enemyPosition.position}");
        }
    }

    void AfficherTourJoueur()
    {
        if (joueurs == null || joueurs.Count == 0)
        {
            Debug.LogError("[Combat] Liste joueurs vide !");
            return;
        }

        if (indexJoueur >= joueurs.Count)
        {
            indexJoueur = 0;
            StartCoroutine(TourEnnemi());
            return;
        }

        PlayerCombatant joueur = joueurs[indexJoueur];
        
        if (joueur == null || joueur.data == null || joueur.data.classeData == null)
        {
            Debug.LogError($"[Combat] Joueur {indexJoueur} invalide !");
            ProchainJoueur();
            return;
        }

        // ✅ Afficher le tour même si KO (le joueur fera automatiquement 0 dégâts)
        Log($"\n--- Tour de {joueur.data.classeData.nomClasse} ---");
        
        if (!joueur.estVivant)
        {
            Log($"💀 {joueur.data.classeData.nomClasse} est KO !");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ACTIONS DU JOUEUR
    // ═══════════════════════════════════════════════════════════
    public void BoutonAttaquer()
    {
        if (actionEnCours) return;
        if (indexJoueur >= joueurs.Count) return;
        
        StartCoroutine(ActionAttaquer());
    }

    public void BoutonDefendre()
    {
        if (actionEnCours) return;
        if (indexJoueur >= joueurs.Count) return;
        
        StartCoroutine(ActionDefendre());
    }

    public void BoutonSoigner()
    {
        if (actionEnCours) return;
        if (indexJoueur >= joueurs.Count) return;
        
        StartCoroutine(ActionSoigner());
    }

    IEnumerator ActionAttaquer()
    {
        actionEnCours = true;
        
        PlayerCombatant joueur = joueurs[indexJoueur];
        
        if (joueur.data == null || joueur.data.classeData == null)
        {
            Debug.LogError("[Combat] Joueur invalide !");
            actionEnCours = false;
            yield break;
        }

        // ✅ Si le joueur est KO, il fait automatiquement 0 dégâts
        if (!joueur.estVivant)
        {
            Log($"{joueur.data.classeData.nomClasse} est KO et ne peut pas attaquer...");
            yield return new WaitForSeconds(1f);
            
            // ✅ Afficher le dé KO
            if (DiceDisplay.Instance != null)
            {
                DiceDisplay.Instance.AfficherDeKO(joueur.data.classeData.nomClasse);
            }
            
            Log("💀 Dé automatique : 0");
            yield return new WaitForSeconds(1f);
            Log("❌ Aucun dégât infligé !");
            yield return new WaitForSeconds(1f);
            
            actionEnCours = false;
            ProchainJoueur();
            yield break;
        }

        Log($"{joueur.data.classeData.nomClasse} attaque !");
        yield return new WaitForSeconds(0.5f);
        
        int de = LancerDe();
        Log($"Dé lancé : {de}");
        
        // ✅ Afficher le dé d'attaque
        bool critique = de >= 5;
        if (DiceDisplay.Instance != null)
        {
            DiceDisplay.Instance.AfficherDeAttaque(de, critique);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (de <= 2)
        {
            Log("❌ Raté !");
        }
        else
        {
            // ✅ Dégâts basiques (à remplacer par système d'armes plus tard)
            int degats = 5 + joueur.data.melee;
            
            if (de >= 5)
            {
                degats = Mathf.RoundToInt(degats * 1.5f);
                Log("⭐ CRITIQUE !");
            }
            
            ennemi.hpActuels -= degats;
            Log($"⚔️ {degats} dégâts ! ({ennemi.hpActuels}/{ennemi.data.pointsDeVie} HP)");
            
            if (ennemi.hpActuels <= 0)
            {
                ennemi.estVivant = false;
                yield return new WaitForSeconds(1f);
                actionEnCours = false;
                Victoire();
                yield break;
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        actionEnCours = false;
        ProchainJoueur();
    }

    IEnumerator ActionDefendre()
    {
        actionEnCours = true;
        
        PlayerCombatant joueur = joueurs[indexJoueur];
        
        if (joueur.data == null || joueur.data.classeData == null)
        {
            Debug.LogError("[Combat] Joueur invalide !");
            actionEnCours = false;
            yield break;
        }

        // ✅ Si le joueur est KO, il ne peut pas se défendre
        if (!joueur.estVivant)
        {
            Log($"{joueur.data.classeData.nomClasse} est KO et ne peut pas se défendre...");
            yield return new WaitForSeconds(1f);
            Log("💀 Dé automatique : 0");
            yield return new WaitForSeconds(1f);
            Log("❌ Défense impossible !");
            yield return new WaitForSeconds(1f);
            
            actionEnCours = false;
            ProchainJoueur();
            yield break;
        }

        Log($"{joueur.data.classeData.nomClasse} se met en défense !");
        yield return new WaitForSeconds(0.5f);
        
        int de = LancerDe();
        Log($"Dé de défense : {de}");
        
        // ✅ Afficher le dé de défense
        bool parfait = de >= 5;
        if (DiceDisplay.Instance != null)
        {
            DiceDisplay.Instance.AfficherDeDefense(de, parfait);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (de <= 2)
        {
            Log("❌ Défense ratée !");
        }
        else
        {
            int soins = 3;
            
            if (de >= 5)
            {
                soins = 5;
                Log("⭐ DÉFENSE PARFAITE !");
            }
            
            int hpMax = joueur.data.classeData.pointsDeVie;
            int ancien = joueur.hpActuels;
            joueur.hpActuels = Mathf.Min(joueur.hpActuels + soins, hpMax);
            int soinsReels = joueur.hpActuels - ancien;
            
            if (soinsReels > 0)
                Log($"🛡️ +{soinsReels} HP ! ({joueur.hpActuels}/{hpMax} HP)");
            else
                Log($"🛡️ Défense réussie mais HP pleins ({joueur.hpActuels}/{hpMax} HP)");
        }
        
        yield return new WaitForSeconds(1f);
        
        actionEnCours = false;
        ProchainJoueur();
    }

    IEnumerator ActionSoigner()
    {
        actionEnCours = true;
        
        PlayerCombatant joueur = joueurs[indexJoueur];
        
        if (joueur.data == null || joueur.data.classeData == null)
        {
            Debug.LogError("[Combat] Joueur invalide !");
            actionEnCours = false;
            yield break;
        }

        // ✅ Si le joueur est KO, il ne peut pas se soigner
        if (!joueur.estVivant)
        {
            Log($"{joueur.data.classeData.nomClasse} est KO et ne peut pas se soigner...");
            yield return new WaitForSeconds(1f);
            Log("💀 Dé automatique : 0");
            yield return new WaitForSeconds(1f);
            Log("❌ Soin impossible !");
            yield return new WaitForSeconds(1f);
            
            actionEnCours = false;
            ProchainJoueur();
            yield break;
        }

        Log($"{joueur.data.classeData.nomClasse} tente de se soigner...");
        yield return new WaitForSeconds(0.5f);
        
        int de = LancerDe();
        Log($"Dé de soin : {de}");
        
        // ✅ Afficher le dé de soin
        bool critique = de >= 5;
        if (DiceDisplay.Instance != null)
        {
            DiceDisplay.Instance.AfficherDeSoin(de, critique);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (de <= 2)
        {
            Log("❌ Soin raté !");
        }
        else
        {
            int soins = 5;
            
            if (de >= 5)
            {
                soins = 8;
                Log("⭐ SOIN CRITIQUE !");
            }
            
            int hpMax = joueur.data.classeData.pointsDeVie;
            int ancien = joueur.hpActuels;
            joueur.hpActuels = Mathf.Min(joueur.hpActuels + soins, hpMax);
            int soinsReels = joueur.hpActuels - ancien;
            
            if (soinsReels > 0)
                Log($"💚 +{soinsReels} HP ! ({joueur.hpActuels}/{hpMax} HP)");
            else
                Log($"💚 Soin réussi mais HP pleins ({joueur.hpActuels}/{hpMax} HP)");
        }
        
        yield return new WaitForSeconds(1f);
        
        actionEnCours = false;
        ProchainJoueur();
    }

    // ═══════════════════════════════════════════════════════════
    // GESTION DES TOURS
    // ═══════════════════════════════════════════════════════════
    void ProchainJoueur()
    {
        indexJoueur++;
        
        if (indexJoueur >= joueurs.Count)
        {
            indexJoueur = 0;
            StartCoroutine(TourEnnemi());
        }
        else
        {
            AfficherTourJoueur();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // TOUR DE L'ENNEMI
    // ═══════════════════════════════════════════════════════════
    IEnumerator TourEnnemi()
    {
        actionEnCours = true;

        if (ennemi == null || ennemi.data == null)
        {
            Debug.LogError("[Combat] Ennemi invalide !");
            combatEnCours = false;
            actionEnCours = false;
            yield break;
        }

        Log($"\n🔴 Tour de {ennemi.data.nom} !");
        yield return new WaitForSeconds(1f);
        
        // Trouver cibles vivantes
        List<PlayerCombatant> vivants = joueurs.Where(j => j != null && j.estVivant).ToList();
        
        if (vivants.Count == 0)
        {
            actionEnCours = false;
            Defaite();
            yield break;
        }
        
        PlayerCombatant cible = vivants[Random.Range(0, vivants.Count)];
        
        if (cible == null || cible.data == null || cible.data.classeData == null)
        {
            Debug.LogError("[Combat] Cible invalide !");
            actionEnCours = false;
            yield break;
        }

        Log($"Cible : {cible.data.classeData.nomClasse}");
        yield return new WaitForSeconds(0.5f);
        
        int de = LancerDe();
        Log($"Dé ennemi : {de}");
        
        // ✅ Afficher le dé ennemi
        bool critique = de >= 5;
        if (DiceDisplay.Instance != null)
        {
            DiceDisplay.Instance.AfficherDeEnnemi(de, critique);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (de <= 2)
        {
            Log("❌ L'ennemi rate !");
        }
        else
        {
            int degats = ennemi.data.degats;
            
            if (de >= 5)
            {
                degats = Mathf.RoundToInt(degats * 1.5f);
                Log("⭐ Critique ennemi !");
            }
            
            cible.hpActuels -= degats;
            Log($"⚔️ {degats} dégâts à {cible.data.classeData.nomClasse} ! ({cible.hpActuels}/{cible.data.classeData.pointsDeVie} HP)");
            
            // ✅ Si le joueur tombe à 0 HP
            if (cible.hpActuels <= 0)
            {
                cible.estVivant = false;
                Log($"💀 {cible.data.classeData.nomClasse} est KO !");
                
                // ✅ CACHER LE SPRITE du joueur KO PENDANT LE COMBAT
                int indexCible = joueurs.IndexOf(cible);
                if (indexCible >= 0 && indexCible < playersInCombat.Length)
                {
                    SpriteRenderer sr = playersInCombat[indexCible].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.enabled = false;
                        Debug.Log($"[Combat] Sprite de {cible.data.classeData.nomClasse} caché");
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(1.5f);
        
        // Vérifier défaite
        vivants = joueurs.Where(j => j != null && j.estVivant).ToList();
        if (vivants.Count == 0)
        {
            actionEnCours = false;
            Defaite();
            yield break;
        }
        
        actionEnCours = false;
        AfficherTourJoueur();
    }

    // ═══════════════════════════════════════════════════════════
    // FIN DU COMBAT
    // ═══════════════════════════════════════════════════════════
    void Victoire()
    {
        combatEnCours = false;
        actionEnCours = true;
        
        Log("\n✅ VICTOIRE !");
        Log($"Vous avez vaincu {ennemi.data.nom} !");
        
        if (combatUI != null)
            combatUI.SetActive(false);
        
        if (ennemyGameObject != null)
            Destroy(ennemyGameObject);
        
        StartCoroutine(RetourDonjon(2f));
    }

    void Defaite()
    {
        combatEnCours = false;
        actionEnCours = true;
        
        Log("\n❌ DÉFAITE !");
        Log("Tous les héros sont tombés...");
        
        if (combatUI != null)
            combatUI.SetActive(false);
        
        StartCoroutine(RetourDonjon(3f));
    }

    IEnumerator RetourDonjon(float delai)
    {
        yield return new WaitForSeconds(delai);
        
        Log("Retour au donjon...");
        
        // ✅ Sauvegarder HP et gérer les KO
        for (int i = 0; i < playersInCombat.Length && i < joueurs.Count; i++)
        {
            if (joueurs[i] != null)
            {
                playersInCombat[i].pointsDeVie = joueurs[i].hpActuels;
                
                // ✅ Si le joueur était KO
                if (joueurs[i].hpActuels <= 0 && !joueurs[i].estVivant)
                {
                    playersInCombat[i].MarquerKO();
                    
                    // Bloquer le mouvement
                    PlayerMovement pm = playersInCombat[i].GetComponent<PlayerMovement>();
                    if (pm != null)
                        pm.peutBouger = false;
                    
                    // ✅ RÉAFFICHER LE SPRITE même si KO
                    // Le joueur est visible mais immobilisé dans le donjon
                    SpriteRenderer sr = playersInCombat[i].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.enabled = true;
                        Debug.Log($"[Combat] Sprite de {playersInCombat[i].name} réaffiché (KO mais visible)");
                    }
                }
                else
                {
                    // ✅ Réactiver le sprite des joueurs vivants
                    SpriteRenderer sr = playersInCombat[i].GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.enabled = true;
                }
            }
        }
        
        // Téléporter retour
        for (int i = 0; i < playersInCombat.Length && i < positionsDonjon.Length; i++)
        {
            playersInCombat[i].transform.position = positionsDonjon[i];
            
            // Réactiver le mouvement sauf si KO
            if (!playersInCombat[i].estKO)
            {
                PlayerMovement pm = playersInCombat[i].GetComponent<PlayerMovement>();
                if (pm != null)
                    pm.peutBouger = true;
            }
        }
        
        // Détruire sprite ennemi
        if (enemyVisualInstance != null)
            Destroy(enemyVisualInstance);

        if (combatUI != null)
            combatUI.SetActive(false);
        
        // Nettoyer
        joueurs.Clear();
        ennemi = null;
        combatEnCours = false;
        actionEnCours = false;
        indexJoueur = 0;
        
        Log("Combat terminé !");
        
        yield return new WaitForSeconds(2f);
        if (logText != null)
            logText.text = "";
        
        Debug.Log("[Combat] === SYSTÈME RÉINITIALISÉ ===");
    }

    int LancerDe()
    {
        return Random.Range(1, 7);
    }

    void Log(string message)
    {
        Debug.Log($"[Combat] {message}");
        
        if (logText != null)
        {
            logText.text += message + "\n";
            
            string[] lines = logText.text.Split('\n');
            if (lines.Length > 15)
            {
                logText.text = string.Join("\n", lines, lines.Length - 15, 15);
            }
        }
    }
}