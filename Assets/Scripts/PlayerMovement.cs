using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public int deplacementsRestants = 0;
    public Vector2Int position; // position sur la grille
    public GameManager gameManager;
    public bool peutLancerDe = false;
    public bool peutBouger = false;
    public CaseManager caseManager;

    // nouveau : direction d'entrée utilisée pour TryInteract et déplacement
    private Vector2Int inputDirection = Vector2Int.zero;

    void Start()
    {
        // NE PAS écraser la position si elle a déjà été définie par le donjon.
        // Si tu veux initialiser la position automatique quand rien n'est défini :
        if (position == Vector2Int.zero && transform.position != Vector3.zero)
        {
            position = new Vector2Int(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y)
            );
            transform.position = new Vector3(position.x + 0.5f, position.y + 0.5f, 0);
        }

        Debug.Log($"[PlayerMovement.Start] {name} posField={position} transform={transform.position}");
    }

    void OnEnable()
    {
        RFIDEventManager.OnRFIDDetected += OnRFIDDetected;
    }

    void OnDisable()
    {
        RFIDEventManager.OnRFIDDetected -= OnRFIDDetected;
    }

    // Remplace les assignments directs de transform depuis l'extérieur
    // par cet appel pour garantir cohérence champs <-> transform.
    public void SetGridPosition(Vector2Int gridPos)
    {
        position = gridPos;
        transform.position = new Vector3(position.x + 0.5f, position.y + 0.5f, 0);
        Debug.Log($"[SetGridPosition] {name} -> {position}");
    }

    void Update()
    {
        // Sauvegarde positions donjon en permanence
        CombatZoneManager.Instance?.SauvegarderPositionsDonjon();

        // Seulement si c'est le joueur actif
        if (gameManager == null) return;
        if (gameManager.joueurs[gameManager.tourActuel] != GetComponent<Player>())
            return;

        // Seulement si le menu de ramassage d'arme n'est pas ouvert
        if (WeaponPickupUI.Instance != null && WeaponPickupUI.Instance.MenuEstOuvert)
        return;

        if (CombatManager.Instance.combatEnCours == true)
        return;

        // Lancer de dé manuel
        if (peutLancerDe && Input.GetKeyDown(KeyCode.D))
        {
            HandleRoll();
            return;
        }

        if (deplacementsRestants > 0 && peutBouger)
        {
            Vector2Int direction = Vector2Int.zero;

            if (Input.GetKeyDown(KeyCode.W)) direction = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.S)) direction = Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.A)) direction = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.D)) direction = Vector2Int.right;

            if (direction != Vector2Int.zero)
            {
                // on enregistre la dernière direction d'entrée (utile pour TryInteract)
                inputDirection = direction;

                Vector2Int nouvellePosition = position + direction;

                // 1) Vérifier porte fermée devant
                DragonDoor door = GetDoor(nouvellePosition);
                if (door != null && !door.isOpen)
                {
                    // 🚫 porte fermée = mur logique
                    return;
                }

                // 2) Vérification des murs (éviter de traverser)
                Case caseCible = GetCase(nouvellePosition);
                if (caseCible != null && caseCible.type == Case.CaseType.Mur)
                {
                    // C'est un mur → bloqué
                    return;
                }


                // 3) Déplacement autorisé
                position = nouvellePosition;
                transform.position = new Vector3(position.x + 0.5f, position.y + 0.5f, 0);

                deplacementsRestants--;

                if (deplacementsRestants <= 0)
                {
                    gameManager.TourSuivant();
                    gameManager.DebutTour();
                }

                else
                {
                    inputDirection = direction;
                }
            }
        }
    }

    void OnRFIDDetected(int lecteur, string role)
    {
        // Vérifier que c'est le joueur actif
        Player playerActif = gameManager.joueurs[gameManager.tourActuel];
        if (playerActif != GetComponent<Player>())
            return; // Pas le joueur actif → ignorer

        // Vérifier que le rôle RFID correspond au joueur actif
        if (role.ToLower() != playerActif.classeData.nomClasse.ToLower())
            return; // mauvais RFID → ignorer

        // Seulement si le menu de ramassage d'arme n'est pas ouvert
        if (WeaponPickupUI.Instance != null && WeaponPickupUI.Instance.MenuEstOuvert)
        return;

        if (CombatManager.Instance.combatEnCours == true)
        return;

        // Rolling dice if allowed: readers 2 or 4
        if (peutLancerDe && (lecteur == 2 || lecteur == 4))
        {
            HandleRoll();
            return;
        }

        // Movement if have moves
        if (deplacementsRestants > 0 && peutBouger)
        {
            Vector2Int direction = Vector2Int.zero;
            switch (lecteur)
            {
                case 1: direction = Vector2Int.up; break;
                case 2: direction = Vector2Int.right; break;
                case 3: direction = Vector2Int.down; break;
                case 4: direction = Vector2Int.left; break;
            }
            if (direction != Vector2Int.zero)
                TryMove(direction);
        }
    }

    void HandleRoll()
    {
        if (!peutLancerDe) return;
        
        Player player = GetComponent<Player>();
            
        if (player != null && player.toursImmobilisation > 0)
        {
            deplacementsRestants = 0;
            peutLancerDe = false;
            
            Debug.Log($"[PlayerMovement] {name} est immobilisé ! Dé = 0");
            
            if (DiceDisplay.Instance != null)
            {
                DiceDisplay.Instance.AfficherDeKO(name);
            }
            
            player.DecrementerImmobilisation();
            StartCoroutine(PasserTourApresKO());
        }
        else
        {
            // Lancer via dé couleur (asynchrone)
            peutLancerDe = false;
            StartCoroutine(gameManager.RequestColorRollCoroutine(result => {
                deplacementsRestants = result;

                Debug.Log("Dé obtenu (via couleur) : " + deplacementsRestants);

                if (DiceDisplay.Instance != null)
                {
                    DiceDisplay.Instance.AfficherDeDeplacement(deplacementsRestants);
                }

                StartCoroutine(DelayAvantDeplacement());
            }));
        }
    }

    void TryMove(Vector2Int direction)
    {
        inputDirection = direction;

        Vector2Int nouvellePosition = position + direction;

        DragonDoor door = GetDoor(nouvellePosition);
        if (door != null && !door.isOpen)
        {
            // 🚫 porte fermée = mur logique
            return;
        }

        Case caseCible = GetCase(nouvellePosition);
        if (caseCible != null && caseCible.type == Case.CaseType.Mur)
        {
            // C'est un mur → bloqué
            return;
        }

        position = nouvellePosition;
        transform.position = new Vector3(position.x + 0.5f, position.y + 0.5f, 0);

        deplacementsRestants--;

        if (deplacementsRestants <= 0)
        {
            gameManager.TourSuivant();
            gameManager.DebutTour();
        }
        else
        {
            inputDirection = direction;
        }
    }

    Case GetCase(Vector2Int pos)
    {
        return caseManager.GetCase(pos);
    }

    DragonDoor GetDoor(Vector2Int pos)
    {
        Vector2 point = new Vector2(pos.x + 0.5f, pos.y + 0.5f);
        Collider2D hit = Physics2D.OverlapPoint(point);
        if (hit == null) return null;
        return hit.GetComponent<DragonDoor>();
    }
    IEnumerator DelayAvantDeplacement()
    {
        peutBouger = false;
        yield return new WaitForSeconds(0.2f);  // Attendre 0.2 sec
        peutBouger = true;
    }

    IEnumerator PasserTourApresKO()
    {
        yield return new WaitForSeconds(2f); // Laisser le temps de voir le dé à 0
        gameManager.TourSuivant();
        gameManager.DebutTour();
    }
    public Vector2Int GetGridPosition()
    {
        return position;
    }

}