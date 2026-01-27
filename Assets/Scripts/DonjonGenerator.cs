using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DonjonGenerator : MonoBehaviour
{
    [Header("Réglages du donjon")]
    public int largeur = 36;
    public int hauteur = 36;

    public int sallesMin = 6;
    public int sallesMax = 12;

    public Vector2Int tailleSalleMin = new Vector2Int(2, 2);
    public Vector2Int tailleSalleMax = new Vector2Int(4, 4);

    public Vector2Int offset = new Vector2Int(-36, -18);

    [Header("Références")]
    public CaseManager caseManager;
    public Tilemap tilemap;
    public TileBase murRuleTile;
    public TileBase solTile;

    private Case[,] grille;
    private List<RectInt> salles = new List<RectInt>();
    private List<Vector2Int> mursASupprimer = new List<Vector2Int>();
    private RectInt salleSpawn;
    private RectInt salleOeuf;
    private RectInt salleDragon;

    public List<Vector2Int> positionsSpawn = new List<Vector2Int>();
    public List<Vector2Int> positionsAccessibles = new List<Vector2Int>();

    void Start()
    {
        if (GameState.Instance != null && GameState.Instance.donjonGenere)
        {
            RestaurerDonjon();
            Debug.Log("[Donjon] Restauration depuis GameState");
        }
        else
        {
            GenererDonjon();
            SauvegarderDonjon();
            GameState.Instance.donjonGenere = true;
            Debug.Log("[Donjon] Génération initiale");
        }

        DessinerTilemap();
        ExporterCases();
        PlacerJoueurs(FindObjectOfType<GameManager>());
    }

    // ==========================================================
    // -------------------  GÉNÉRATION COMPLETE  -----------------
    // ==========================================================
    void GenererDonjon()
    {
        grille = new Case[largeur, hauteur];
        salles.Clear();

        // 1) Remplir en murs
        for (int x = 0; x < largeur; x++)
            for (int y = 0; y < hauteur; y++)
                grille[x, y] = new Case(new Vector2Int(x, y), Case.CaseType.Mur);

        // 2) Salles fixes
        salleSpawn  = new RectInt(0, hauteur / 2 - 3, 4, 4);
        salleOeuf   = new RectInt(largeur - 10, hauteur - 10, 8, 8);
        salleDragon = new RectInt(largeur - 10, 2, 8, 8);

        salles.Add(salleSpawn);
        salles.Add(salleOeuf);
        salles.Add(salleDragon);

        RemplirSalle(salleSpawn);
        RemplirSalle(salleOeuf);
        RemplirSalle(salleDragon);

        // 3) Génération salles aléatoires
        int nbSalles = Random.Range(sallesMin, sallesMax + 1);
        for (int i = 0; i < nbSalles; i++)
            GenererSalleAleatoire();

        // 4) Relier salles (sauf dragon) avec MST
        RelierSalles();

        // Relié dragon à UNE seule salle aléatoire
        RelierDragon();

        // 5) Construction du caseManager + spawn
        ExporterCases();

        // 6) Nettoyage + dessin
        NettoyerMursIsoles();
        DessinerTilemap();
    }

    void SauvegarderDonjon()
    {
        DonjonData data = new DonjonData
        {
            largeur = largeur,
            hauteur = hauteur,
            offset = offset,
            salleSpawn = salleSpawn,
            salleOeuf = salleOeuf,
            salleDragon = salleDragon,
            typeCases = new Case.CaseType[largeur, hauteur]
        };

        for (int x = 0; x < largeur; x++)
        for (int y = 0; y < hauteur; y++)
            data.typeCases[x, y] = grille[x, y].type;

        GameState.Instance.donjonData = data;
    }

    void RestaurerDonjon()
    {
        DonjonData data = GameState.Instance.donjonData;

        largeur = data.largeur;
        hauteur = data.hauteur;
        offset = data.offset;

        salleSpawn = data.salleSpawn;
        salleOeuf = data.salleOeuf;
        salleDragon = data.salleDragon;

        grille = new Case[largeur, hauteur];

        for (int x = 0; x < largeur; x++)
        for (int y = 0; y < hauteur; y++)
            grille[x, y] = new Case(new Vector2Int(x, y), data.typeCases[x, y]);

        NettoyerMursIsoles();
    }

    // ==========================================================
    // ------------------------  SALLES  -------------------------
    // ==========================================================
    void GenererSalleAleatoire()
    {
        for (int essai = 0; essai < 50; essai++)
        {
            int w = Random.Range(tailleSalleMin.x, tailleSalleMax.x + 1);
            int h = Random.Range(tailleSalleMin.y, tailleSalleMax.y + 1);
            int px = Random.Range(2, largeur - w - 2);
            int py = Random.Range(2, hauteur - h - 2);

            RectInt nouvelle = new RectInt(px, py, w, h);
            if (!SalleValide(nouvelle)) continue;

            salles.Add(nouvelle);
            RemplirSalle(nouvelle);
            return;
        }
    }

    bool SalleValide(RectInt salle)
    {
        foreach (var s in salles)
        {
            RectInt zone = new RectInt(s.xMin - 4, s.yMin - 4, s.width + 4, s.height + 4);
            if (zone.Overlaps(salle))
                return false;
        }
        return true;
    }

    void RemplirSalle(RectInt s)
    {
        for (int x = s.xMin; x <= s.xMax; x++)
            for (int y = s.yMin; y <= s.yMax; y++)
                grille[x, y].type = Case.CaseType.Chemin;
    }

    // ==========================================================
    // ----------------------  COULOIRS  -------------------------
    // ==========================================================
    void RelierSalles()
    {
        // Prim : relie chaque salle à la plus proche non reliée
        List<RectInt> nonRelie = new List<RectInt>(salles);
        nonRelie.Remove(salleDragon); // dragon isolé

        List<RectInt> arbre = new List<RectInt> { nonRelie[0] };
        nonRelie.RemoveAt(0);

        while (nonRelie.Count > 0)
        {
            float bestDist = float.MaxValue;
            RectInt A = arbre[0], B = nonRelie[0];

            foreach (var sA in arbre)
            foreach (var sB in nonRelie)
            {
                float d = Vector2Int.Distance(Centre(sA), Centre(sB));
                if (d < bestDist)
                {
                    A = sA; B = sB; bestDist = d;
                }
            }

            CreerCouloir(Centre(A), Centre(B), false);
            arbre.Add(B); 
            nonRelie.Remove(B);
        }
    }

    void RelierDragon()
    {
        // On crée une liste des salles "candidates"
        // → pas la salle dragon elle-même
        // → uniquement les salles déjà reliées entre elles
        List<RectInt> candidates = new List<RectInt>(salles);
        candidates.Remove(salleDragon);

        // Choisit UNE salle au hasard
        RectInt cible = candidates[Random.Range(0, candidates.Count)];

        // Trace 1 couloir → c’est le seul vers la salle Dragon
        CreerCouloir(Centre(salleDragon), Centre(cible), true);
    }

    Vector2Int Centre(RectInt s) =>
        new Vector2Int(s.xMin + s.width / 2, s.yMin + s.height / 2);

    bool EstZoneInterditePourCouloir(int x, int y)
    {
        // On élargit la zone d'un tile pour éviter les touches accidentelles
        RectInt interdit = new RectInt(
            salleDragon.xMin - 3,
            salleDragon.yMin - 3,
            salleDragon.width + 3,
            salleDragon.height + 3
        );

        return interdit.Contains(new Vector2Int(x, y));
    }
    void CreerCouloir(Vector2Int a, Vector2Int b, bool forcer = false)
    {
        int x = a.x;
        int y = a.y;

        while (x != b.x)
        {
            if (forcer || !EstZoneInterditePourCouloir(x, y))
                grille[x, y].type = Case.CaseType.Chemin;

            x += (b.x > x) ? 1 : -1;
        }

        while (y != b.y)
        {
            if (forcer || !EstZoneInterditePourCouloir(x, y))
                grille[x, y].type = Case.CaseType.Chemin;

            y += (b.y > y) ? 1 : -1;
        }
    }

    // ==========================================================
    // -----------------------  EXPORT  --------------------------
    // ==========================================================
    void ExporterCases()
    {
        caseManager.cases.Clear();
        positionsSpawn.Clear();
        positionsAccessibles.Clear();

        for (int x = 0; x < largeur; x++)
        for (int y = 0; y < hauteur; y++)
        {
            Vector2Int pos = new Vector2Int(x, y) + offset;
            caseManager.cases[pos] = grille[x, y];
            if (grille[x, y].type == Case.CaseType.Chemin)
                positionsAccessibles.Add(pos);
        }

        // Spawn
        for (int x = salleSpawn.xMin; x <= salleSpawn.xMax; x++)
            for (int y = salleSpawn.yMin; y <= salleSpawn.yMax; y++)
                positionsSpawn.Add(new Vector2Int(x, y) + offset);
    }

    // ==========================================================
    // ----------------------  NETTOYAGE  ------------------------
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
    // ----------------  ACCESSIBILITÉ DE L’ŒUF  -----------------
    // ==========================================================
    bool OeufAccessibleAvantDragon()
    {
        Queue<Vector2Int> file = new Queue<Vector2Int>();
        HashSet<Vector2Int> visite = new HashSet<Vector2Int>();

        Vector2Int start = Centre(salleSpawn);
        file.Enqueue(start);

        while (file.Count > 0)
        {
            Vector2Int p = file.Dequeue();
            if (!visite.Add(p)) continue;

            if (salleOeuf.Contains(p)) return true;
            if (salleDragon.Contains(p)) continue;

            foreach (var v in new Vector2Int[] { p + Vector2Int.up, p + Vector2Int.down, p + Vector2Int.left, p + Vector2Int.right })
            {
                if (v.x >= 0 && v.x < largeur && v.y >= 0 && v.y < hauteur)
                    if (grille[v.x, v.y].type == Case.CaseType.Chemin)
                        file.Enqueue(v);
            }
        }
        return false;
    }

    // ==========================================================
    // -----------------------  SPAWN  ---------------------------
    // ==========================================================
    void PlacerJoueurs(GameManager gm)
    {
        if (gm == null || positionsSpawn.Count == 0) return;

        List<Vector2Int> libres = new List<Vector2Int>(positionsSpawn);

        foreach (var j in gm.joueurs)
        {
            PlayerData savedData = GameState.Instance.joueurs.Find(p => p.classeData == j.classeData);
            Vector2Int sp;

            if (savedData != null)
            {
                sp = savedData.position; // Restaurer la position
            }
            else
            {
                int idx = Random.Range(0, libres.Count);
                sp = libres[idx];
                libres.RemoveAt(idx);
            }

            var pm = j.GetComponent<PlayerMovement>();
            if (pm != null) pm.SetGridPosition(sp);
            else j.transform.position = new Vector3(sp.x + 0.5f, sp.y + 0.5f, 0);

            // Restaurer stats et inventaire
            j.pointsDeVie = savedData?.pointsDeVie ?? j.pointsDeVie;
            j.melee = savedData?.melee ?? j.melee;
            j.distance = savedData?.distance ?? j.distance;
            j.crochetage = savedData?.crochetage ?? j.crochetage;
            j.arme1 = savedData?.arme1 ?? j.arme1;
            j.arme2 = savedData?.arme2 ?? j.arme2;
        }
    }

    public RectInt GetSalleDragon() => salleDragon;
    public RectInt GetSalleOeuf() => salleOeuf;

    public RectInt GetSalleSpawn() => salleSpawn;
    public Case[,] GetGrille() => grille;
    public Vector2Int GetOffset() => offset;

}
