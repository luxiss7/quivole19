using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ClasseSelectionManager : MonoBehaviour
{
    public List<ClasseData> classes; // ScriptableObjects
    public Player[] joueurs;         // joueurs du GameManager
    public float tempsAffichage = 3f;
    public GameManager gameManager;  // référence à ton GameManager

    private int indexClasse = 0;
    private float timerInactivite = 0f;
    private bool selectionActive = true;

    [Header("UI")]
    public Image classeActuelleImage;      // Sprite en rotation au centre
    public Image[] portraitsJoueurs;       // 4 images pour Joueur 1 à 4

    void Start()
    {
        if (gameManager == null)
            Debug.LogWarning("GameManager non assigné !");

        StartCoroutine(DefilementClasses());
    }

    void Update()
    {
        if (!selectionActive)
            return;

        timerInactivite += Time.deltaTime;

        // Si 2 joueurs ou + ont choisi ET 30 sec d'inactivité → Lancer partie
        if (NombreJoueursChoisis() >= 2 && timerInactivite >= 30f)
        {
            Debug.Log("AUTO-LAUNCH : Personne n’a choisi depuis 30 sec. Début de la partie !");
            LancerPartie();
        }

        DetecterInput();
    }

    IEnumerator DefilementClasses()
    {
        while (selectionActive)
        {
            // ÉVITE LE CRASH : si aucune classe restante → fin sélection
            if (classes.Count == 0)
            {
                Debug.Log("Plus aucune classe disponible !");
                LancerPartie();
                yield break;
            }

            if (indexClasse >= classes.Count)
                indexClasse = 0;

            AfficherClasse(classes[indexClasse]);

            yield return new WaitForSeconds(tempsAffichage);

            indexClasse = (indexClasse + 1) % classes.Count;
        }
    }

    void AfficherClasse(ClasseData classe)
    {
        // UI
        if (classeActuelleImage != null)
            classeActuelleImage.sprite = classe.sprite;

        // Debug facultatif
        Debug.Log("Classe affichée : " + classe.nomClasse);

        for (int i = 0; i < joueurs.Length; i++)
        {
            if (joueurs[i].classeData != null)
                Debug.Log($"Joueur {i + 1} a choisi {joueurs[i].classeData.nomClasse}");
            else
                Debug.Log($"Joueur {i + 1} n'a pas encore choisi de classe");
        }
    }

    void OnEnable()
    {
        RFIDEventManager.OnRFIDDetected += OnRFIDDetected;
    }

    void OnDisable()
    {
        RFIDEventManager.OnRFIDDetected -= OnRFIDDetected;
    }

    void DetecterInput()
    {
        if (classes.Count == 0)
            return;

        ClasseData classeActuelle = classes[indexClasse];

        if (Input.GetKeyDown(KeyCode.W)) ChoisirClasse(0, classeActuelle);
        if (Input.GetKeyDown(KeyCode.D)) ChoisirClasse(1, classeActuelle);
        if (Input.GetKeyDown(KeyCode.S)) ChoisirClasse(2, classeActuelle);
        if (Input.GetKeyDown(KeyCode.A)) ChoisirClasse(3, classeActuelle);
    }

    void OnRFIDDetected(int lecteur, string role)
    {
        if (!selectionActive || classes.Count == 0)
            return;

        int joueurIndex;
        switch (lecteur)
        {
            case 1: joueurIndex = 0; break;
            case 2: joueurIndex = 1; break;
            case 3: joueurIndex = 2; break;
            case 4: joueurIndex = 3; break;
            default:
                Debug.Log("Lecteur RFID inconnu : " + lecteur);
                return;
        }

        ClasseData detected = null;

        switch (roleKey)
        {
            case "voleur":
                detected = classes.Find(c => string.Equals(c.nomClasse, "Voleur", StringComparison.OrdinalIgnoreCase));
                break;
            case "archer":
                detected = classes.Find(c => string.Equals(c.nomClasse, "Archer", StringComparison.OrdinalIgnoreCase));
                break;
            case "tank":
                detected = classes.Find(c => string.Equals(c.nomClasse, "Tank", StringComparison.OrdinalIgnoreCase));
                break;
            case "soigneur":
                detected = classes.Find(c => string.Equals(c.nomClasse, "Soigneur", StringComparison.OrdinalIgnoreCase));
                break;
            default:
                Debug.Log($"Role RFID inconnu : {role}");
                return;
        }

        if (detected == null)
        {
            Debug.Log($"Classe détectée '{role}' non trouvée.");
            return;
        }

        ChoisirClasse(joueurIndex, detected);
    }

    void ChoisirClasse(int joueurIndex, ClasseData classe)
    {
        if (joueurIndex >= joueurs.Length)
            return;

        // Vérifier si déjà pris
        foreach (Player p in joueurs)
        {
            if (p.classeData == classe)
            {
                Debug.Log("Classe déjà prise !");
                return;
            }
        }

        // Joueur déjà choisi ?
        if (joueurs[joueurIndex].classeData != null)
        {
            Debug.Log("Joueur " + (joueurIndex + 1) + " a déjà une classe.");
            return;
        }

        // Assignation
        joueurs[joueurIndex].classeData = classe;
        joueurs[joueurIndex].ChargerClasse();
        timerInactivite = 0f;

        Debug.Log($"Joueur {joueurIndex + 1} a choisi {classe.nomClasse}");

        // 🔥🔥🔥 UI : mettre le sprite dans la carte
        if (portraitsJoueurs[joueurIndex] != null)
            portraitsJoueurs[joueurIndex].sprite = classe.sprite;

        // Retirer la classe disponible
        if (classes.Count > 1)
        {
            classes.Remove(classe);
        }

        indexClasse = 0;

        if (NombreJoueursChoisis() == joueurs.Length)
        {
            Debug.Log("Tous les joueurs ont choisi !");
            LancerPartie();
        }
    }

    int NombreJoueursChoisis()
    {
        int count = 0;
        foreach (var j in joueurs)
            if (j.classeData != null)
                count++;

        return count;
    }

    void LancerPartie()
    {
        selectionActive = false;

        // 🔥 SUPPRESSION DES JOUEURS SANS CLASSE
        List<Player> joueursValides = new List<Player>();

        foreach (var j in joueurs)
        {
            if (j.classeData != null)
            {
                joueursValides.Add(j);
            }
            else
            {
                Debug.Log($"Suppression du joueur {j.name}, aucune classe choisie.");
                Destroy(j.gameObject);
            }
        }

        // Mise à jour du GameManager
        gameManager.joueurs = joueursValides;

        Debug.Log("La partie COMMENCE maintenant !");

        if (gameManager != null)
        {
            gameManager.classesChoisies = true;
            gameManager.DebutTour();

            if (gameManager.classChoiceUI != null)
            gameManager.classChoiceUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameManager non assigné ! Impossible de lancer la partie.");
        }
    }
}