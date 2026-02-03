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
            // ✅ NOUVEAU : Vérifier si le joueur est immobilisé (KO)
            Player player = GetComponent<Player>();
            
            if (player != null && player.toursImmobilisation > 0)
            {
                // ✅ Le joueur est immobilisé → Dé automatique = 0
                deplacementsRestants = 0;
                peutLancerDe = false;
                
                Debug.Log($"[PlayerMovement] {name} est immobilisé ! Dé = 0");
                
                // ✅ Afficher le dé KO
                if (DiceDisplay.Instance != null)
                {
                    DiceDisplay.Instance.AfficherDeKO(name);
                }
                
                // ✅ Décrémenter l'immobilisation
                player.DecrementerImmobilisation();
                
                // ✅ Passer au tour suivant immédiatement
                StartCoroutine(PasserTourApresKO());
            }
            else
            {
                // ✅ Joueur normal → Lancer le dé normalement
                deplacementsRestants = gameManager.LancerDe();
                peutLancerDe = false;

                Debug.Log("Dé obtenu : " + deplacementsRestants);
                
                // ✅ Afficher le dé de déplacement
                if (DiceDisplay.Instance != null)
                {
                    DiceDisplay.Instance.AfficherDeDeplacement(deplacementsRestants);
                }

                StartCoroutine(DelayAvantDeplacement());
            }
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