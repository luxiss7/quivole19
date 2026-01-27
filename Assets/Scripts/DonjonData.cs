using UnityEngine;

[System.Serializable]
public class DonjonData
{
    public int largeur;
    public int hauteur;
    public Vector2Int offset;

    public RectInt salleSpawn;
    public RectInt salleOeuf;
    public RectInt salleDragon;

    public Case.CaseType[,] typeCases;
}
