using UnityEngine;

[System.Serializable]
public class Case
{
    public enum CaseType { Chemin, Mur, Porte }

    public Vector2Int position;
    public CaseType type;

    public Case(Vector2Int pos, CaseType t)
    {
        position = pos;
        type = t;
    }
}
