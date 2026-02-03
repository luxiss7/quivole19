using UnityEngine;
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

    // Lancer de dé
    public int LancerDe()
    {
        // int resultat = Random.Range(1, 7); // 1 à 6
        int resultat = Random.Range(10, 70); // test
        Debug.Log("Dé lancé : " + resultat);
        return resultat;
    }

    IEnumerator DelaiAvantAutoriserLancer(PlayerMovement pm)
    {
        yield return new WaitForSeconds(0.2f); // anti-trigger
        pm.peutLancerDe = true; // le joueur peut maintenant lancer son dé
    }

}