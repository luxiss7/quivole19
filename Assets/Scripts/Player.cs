using UnityEngine;

/// <summary>
/// Classe joueur avec gestion du système de KO
/// Nouvelles fonctionnalités :
/// - Suivi de l'état KO (0 HP)
/// - Immobilisation pendant 1 tour si KO au retour du donjon
/// - Régénération HP pleins au prochain combat
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Choix de la classe")]
    public ClasseData classeData;

    [Header("Stats finales du joueur")]
    public int pointsDeVie;
    public int melee;
    public int distance;
    public int crochetage;
    public WeaponData arme1;
    public WeaponData arme2;

    public Vector2Int position;

    [Header("État KO")]
    public bool estKO = false; // ✅ NOUVEAU : Indique si le joueur est KO
    public int toursImmobilisation = 0; // ✅ NOUVEAU : Nombre de tours restants d'immobilisation

    void Start()
    {
        if (classeData != null)
            ChargerClasse();
    }

    void Update()
    {
        // ✅ NOUVEAU : Gérer l'immobilisation
        if (toursImmobilisation > 0)
        {
            PlayerMovement pm = GetComponent<PlayerMovement>();
            if (pm != null)
                pm.peutBouger = false;
        }
    }

    public void ChargerClasse()
    {
        pointsDeVie = classeData.pointsDeVie;
        melee = classeData.melee;
        distance = classeData.distance;
        crochetage = classeData.crochetage;

        arme1 = classeData.arme1;
        arme2 = classeData.arme2;

        // appliquer sprite
        GetComponent<SpriteRenderer>().sprite = classeData.sprite;
    }

    public PlayerData CreerPlayerData()
    {
        PlayerData data = new PlayerData
        {
            pointsDeVie = pointsDeVie,
            melee = melee,
            distance = distance,
            crochetage = crochetage,
            arme1 = arme1,
            arme2 = arme2,
            classeData = classeData
        };

        PlayerInventory inv = GetComponent<PlayerInventory>();
        if (inv != null)
            data.hasDragonKey = inv.hasDragonKey;

        return data;
    }

    /// <summary>
    /// ✅ NOUVEAU : Marquer le joueur comme KO
    /// Appelé par le CombatManager quand HP <= 0
    /// </summary>
    public void MarquerKO()
    {
        estKO = true;
        toursImmobilisation = 1; // Immobilisé pendant 1 tour au retour
        Debug.Log($"[Player] {name} est KO ! Sera immobilisé 1 tour.");
    }

    /// <summary>
    /// ✅ NOUVEAU : Réanimer le joueur pour le prochain combat
    /// Appelé par le CombatManager au début d'un nouveau combat
    /// </summary>
    public void Reanimer()
    {
        if (estKO)
        {
            pointsDeVie = classeData.pointsDeVie; // HP pleins
            estKO = false;
            Debug.Log($"[Player] {name} réanimé avec {pointsDeVie} HP !");
        }
    }

    /// <summary>
    /// ✅ NOUVEAU : Décrémenter l'immobilisation à chaque tour
    /// Appelé par le système de mouvement ou TurnManager
    /// </summary>
    public void DecrementerImmobilisation()
    {
        if (toursImmobilisation > 0)
        {
            toursImmobilisation--;
            
            if (toursImmobilisation == 0)
            {
                Debug.Log($"[Player] {name} peut de nouveau bouger !");
                
                // Réactiver le mouvement
                PlayerMovement pm = GetComponent<PlayerMovement>();
                if (pm != null)
                    pm.peutBouger = true;
            }
            else
            {
                Debug.Log($"[Player] {name} encore immobilisé pour {toursImmobilisation} tour(s).");
            }
        }
    }

    /// <summary>
    /// ✅ NOUVEAU : Vérifier si le joueur peut bouger
    /// </summary>
    public bool PeutBouger()
    {
        return toursImmobilisation == 0;
    }
}