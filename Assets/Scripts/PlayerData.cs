using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public ClasseData classeData;

    public int pointsDeVie;
    public int melee;
    public int distance;
    public int crochetage;

    public WeaponData arme1;
    public WeaponData arme2;

    public bool hasDragonKey;
    public Vector2Int position;
}
