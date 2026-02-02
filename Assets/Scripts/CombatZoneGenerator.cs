using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CombatZoneGenerator : MonoBehaviour
{
    [Header("Réglages")]
    public int largeur = 36;
    public int hauteur = 36;
    public int tailleZone = 8;

    public Vector2Int offset = new Vector2Int(-18, -18);

    [Header("Références")]
    public CaseManager caseManager;
    public Tilemap tilemap;
    public TileBase murRuleTile;
    public TileBase solTile;

    private Case[,] grille;
    private RectInt zoneCombat;
    
    private List<Vector2Int> mursASupprimer = new List<Vector2Int>();

    void Start()
    {
        GenererZoneCombat();
        NettoyerMursIsoles();
        DessinerTilemap();
        ExporterCases();
    }

    // ==========================================================
    // -------------------  GÉNÉRATION  --------------------------
    // ==========================================================
    void GenererZoneCombat()
    {
        grille = new Case[largeur, hauteur];

        // 1) Tout en murs
        for (int x = 0; x < largeur; x++)
        for (int y = 0; y < hauteur; y++)
            grille[x, y] = new Case(new Vector2Int(x, y), Case.CaseType.Mur);

        // 2) Zone de combat centrée
        int cx = largeur / 2 - tailleZone / 2;
        int cy = hauteur / 2 - tailleZone / 2;
        zoneCombat = new RectInt(cx, cy, tailleZone, tailleZone);

        // 3) Remplir la zone en sol
        for (int x = zoneCombat.xMin; x < zoneCombat.xMax; x++)
        for (int y = zoneCombat.yMin; y < zoneCombat.yMax; y++)
            grille[x, y].type = Case.CaseType.Chemin;
    }

    // ==========================================================
    // -----------------------  EXPORT  --------------------------
    // ==========================================================
    void ExporterCases()
    {
        caseManager.cases.Clear();

        for (int x = 0; x < largeur; x++)
        for (int y = 0; y < hauteur; y++)
        {
            Vector2Int pos = new Vector2Int(x, y) + offset;
            caseManager.cases[pos] = grille[x, y];
        }
    }

    // ==========================================================
    // -----------------------  TILEMAP  -------------------------
    // ==========================================================
    void NettoyerMursIsoles()
    {
        mursASupprimer.Clear();
        for (int x = 0; x < largeur; x++)
        for (int y = 0; y < hauteur; y++)
        {
            if (grille[x, y].type != Case.CaseType.Mur) continue;

            bool isole = true;
            for (int nx = -1; nx <= 1; nx++)
            for (int ny = -1; ny <= 1; ny++)
            {
                if (nx == 0 && ny == 0) continue;
                int xx = x + nx, yy = y + ny;
                if (xx < 0 || xx >= largeur || yy < 0 || yy >= hauteur) continue;
                if (grille[xx, yy].type == Case.CaseType.Chemin)
                    isole = false;
            }
            if (isole) mursASupprimer.Add(new Vector2Int(x, y));
        }
    }

    void DessinerTilemap()
    {
        tilemap.ClearAllTiles();
        for (int x = 0; x < largeur; x++)
        for (int y = 0; y < hauteur; y++)
        {
            Vector3Int pos = new Vector3Int(x + offset.x, y + offset.y, 0);

            if (grille[x, y].type == Case.CaseType.Mur)
            {
                if (!mursASupprimer.Contains(new Vector2Int(x, y)))
                    tilemap.SetTile(pos, murRuleTile);
            }
            else tilemap.SetTile(pos, solTile);
        }
    }

    // ==========================================================
    // -----------------------  GETTERS  -------------------------
    // ==========================================================
    public RectInt GetZoneCombat() => zoneCombat;
    public Case[,] GetGrille() => grille;
    public Vector2Int GetOffset() => offset;
}
