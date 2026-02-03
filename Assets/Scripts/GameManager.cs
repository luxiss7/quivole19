using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<Player> joueurs; // Liste des joueurs dans la partie
    public int tourActuel = 0;   // Quel joueur est actif
    public bool classesChoisies = false; // nouvelle variable
    public SmoothCameraFollow cameraPrincipale;
    public GameObject classChoiceUI;   // Le GameObject qui contient ton interface de choix
    public bool weaponPickupAutorise = true;

    [Header("Dice Scan UI")]
    public GameObject diceScanUI;       // UI à activer pendant l'attente du roll couleur
    public Text diceScanText;           // Texte à afficher (optionnel)
    public string diceScanMessage = "Lancez le dé..."; // Message par défaut

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (GameState.Instance != null && GameState.Instance.donjonGenere) return;
        if (joueurs.Count == 0)
        {
            Debug.LogWarning("Aucun joueur n'est défini !");
        }
        else
        {
            // Attendre le choix de classe avant de commencer le premier tour
            StartCoroutine(AttendreChoixClasse());
        }

        // Ensure dice scan UI hidden at start
        if (diceScanUI != null)
            diceScanUI.SetActive(false);
    }



    IEnumerator AttendreChoixClasse()
    {
        // Boucle tant que tous les joueurs n'ont pas choisi
        while (!classesChoisies)
        {
            yield return null; // attendre 1 frame
        }

        // Une fois les classes choisies, on démarre le premier tour
        DebutTour();
    }


    public void DebutTour()
    {
        // On autorise à nouveau le pickup au debut du tour
        weaponPickupAutorise = true;

        Player joueurActif = joueurs[tourActuel];
        PlayerMovement pm = joueurActif.GetComponent<PlayerMovement>();

        pm.deplacementsRestants = 0;
        pm.peutBouger = false;
        pm.peutLancerDe = false; // on bloque

        StartCoroutine(DelaiAvantAutoriserLancer(pm));
        MettreAJourCamera();

        Debug.Log("Tour du joueur : " + joueurActif.classeData.nomClasse + " - Appuyer sur D pour lancé le dé");
    }

    public void MettreAJourCamera()
    {
        if (cameraPrincipale == null) return;

        // Si un combat est en cours → caméra sur l'ennemi
        if (CombatManager.Instance != null && CombatManager.Instance.combatEnCours)
        {
            Transform cibleCombat = CombatManager.Instance.GetCameraTarget();
            if (cibleCombat != null)
                cameraPrincipale.cible = cibleCombat;
        }
        else
        {
            // Sinon → caméra sur le joueur actif
            DeplacerCameraVersJoueur(tourActuel);
        }
    }

    void DeplacerCameraVersJoueur(int index)
    {
        Player joueurActif = joueurs[index];

        if (cameraPrincipale != null)
            cameraPrincipale.cible = joueurActif.transform;
    }

    // Passe au joueur suivant
    public void TourSuivant()
    {
        tourActuel++;
        if (tourActuel >= joueurs.Count)
            tourActuel = 0;

        MettreAJourCamera();
        Debug.Log("Tour du joueur : " + joueurs[tourActuel].classeData.nomClasse);
    }

    // Lancer de dé (fallback random, utilisé si jamais nécessaire)
    public int LancerDe()
    {
        int resultat = Random.Range(1, 7); // 1 à 6
        Debug.Log("Dé lancé (fallback random): " + resultat);
        return resultat;
    }

    IEnumerator DelaiAvantAutoriserLancer(PlayerMovement pm)
    {
        yield return new WaitForSeconds(0.2f); // anti-trigger
        pm.peutLancerDe = true; // le joueur peut maintenant lancer son dé
    }

    // Demande asynchrone d'un roll via dé couleur (pas de timeout - attend indéfiniment)
    // onResult reçoit la valeur 1..6 quand l'événement couleur est détecté.
    public IEnumerator RequestColorRollCoroutine(System.Action<int> onResult)
    {
        int? result = null;

        void Handler(string color, int value)
        {
            // On accepte la valeur telle quelle, en s'assurant qu'elle est entre 1 et 6
            result = Mathf.Clamp(value, 1, 6);
        }

        // Afficher l'UI d'attente si présente
        if (diceScanUI != null)
        {
            diceScanUI.SetActive(true);
            if (diceScanText != null)
                diceScanText.text = diceScanMessage;
        }

        ColorEventManager.OnColorDetected += Handler;

        // Attendre l'événement (sans timeout, comme demandé)
        while (result == null)
            yield return null;

        // Se désabonner puis appeler le callback
        ColorEventManager.OnColorDetected -= Handler;

        // Masquer l'UI d'attente
        if (diceScanUI != null)
            diceScanUI.SetActive(false);

        onResult?.Invoke(result.Value);
    }

}