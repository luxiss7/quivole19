using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaseManager : MonoBehaviour
{
    public Tilemap tilemap;  
    public TileBase murTile; // la tile qui représente un mur

    public Dictionary<Vector2Int, Case> cases = new Dictionary<Vector2Int, Case>();

    void Start()
    {
        LireTilemap();
    }

    void LireTilemap()
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int posTilemap = new Vector3Int(pos.x, pos.y, 0);
            TileBase tile = tilemap.GetTile(posTilemap);

            if (tile != null)
            {
                Vector2Int posCase = new Vector2Int(pos.x, pos.y);

                Case.CaseType type = Case.CaseType.Chemin;

                if (tile == murTile)
                    type = Case.CaseType.Mur;

                cases[posCase] = new Case(posCase, type);
            }
        }

        Debug.Log("Tilemap lue : " + cases.Count + " cases enregistrées.");
    }

    public Case GetCase(Vector2Int pos)
    {
        if (cases.ContainsKey(pos))
            return cases[pos];

        return null;
    }
}
