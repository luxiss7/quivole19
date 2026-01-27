using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Choix de la classe")]
    public ClasseData classeData;

    [Header("Stats finales du joueur")]
    public int pointsDeVie;
    public int melee;
    public int distance;
    public int crochetage;
    public Weapon arme1;
    public Weapon arme2;

    public Vector2Int position;

    void Start()
    {
        if (classeData != null)
            ChargerClasse();
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

}
