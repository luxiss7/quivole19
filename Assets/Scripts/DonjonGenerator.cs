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
        bool valide = false;
        int essais = 0;

        while (!valide && essais < 20)
        {
            essais++;
            GenererDonjonInterne();
            valide = OeufAccessibleAvantDragon();
        }

        if (!valide)
            Debug.LogError("Donjon invalide après plusieurs tentatives");
    }

    void GenererDonjonInterne()
    {
        grille = new Case[largeur, hauteur];
        salles.Clear();

        // 1) Remplir en murs
        for (int x = 0; x < largeur; x++)
            for (int y = 0; y < hauteur; y++)
                grille[x, y] = new Case(new Vector2Int(x, y), Case.CaseType.Mur);

        // 2) Salles fixes
        salleSpawn  = new RectInt(2, hauteur / 2 - 3, 4, 4);
        salleOeuf   = new RectInt(largeur - 10, hauteur - 10, 8, 8);
        salleDragon = new RectInt(largeur - 10, 2, 8, 8);

        salles.Add(salleSpawn);
        salles.Add(salleOeuf);
        salles.Add(salleDragon);

        RemplirSalle(salleSpawn);
        RemplirSalle(salleOeuf);
        RemplirSalle(salleDragon);

        // 3) Salles aléatoires
        int nbSalles = Random.Range(sallesMin, sallesMax + 1);
        for (int i = 0; i < nbSalles; i++)
            GenererSalleAleatoire();

        // 4) Connexions
        RelierSalles();
        RelierDragon();

        // 5) Nettoyage
        NettoyerMursIsoles();
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

    // ★ NOUVEAU : gestion rayon spécial
    bool EstSalleSpeciale(RectInt s)
    {
        return RectIdentique(s, salleSpawn)
            || RectIdentique(s, salleOeuf)
            || RectIdentique(s, salleDragon);
    }

    bool RectIdentique(RectInt a, RectInt b)
    {
        return a.xMin == b.xMin &&
            a.yMin == b.yMin &&
            a.width == b.width &&
            a.height == b.height;
    }

    public static bool RectContainsInclusive(RectInt r, Vector2Int pos)
    {
        return pos.x >= r.xMin && pos.x <= r.xMin + r.width - 1 &&
            pos.y >= r.yMin && pos.y <= r.yMin + r.height - 1;
    }

    public bool EstCaseDansSalleSpeciale(Vector2Int pos)
    {
        // 1) Bordures du donjon (marge 1 case)
        if (pos.x <= 1 || pos.y <= 1 || pos.x >= largeur - 2 || pos.y >= hauteur - 2)
            return true;

        // 2) Salles fixes (avec bord inclusif)
        if (RectContainsInclusive(salleSpawn, pos) ||
            RectContainsInclusive(salleOeuf, pos) ||
            RectContainsInclusive(salleDragon, pos))
            return true;

        return false;
    }

    bool SalleValide(RectInt salle)
    {
        foreach (var s in salles)
        {
            int rayon = EstSalleSpeciale(s) ? 4 : 1;

            RectInt zone = new RectInt(
                s.xMin - rayon,
                s.yMin - rayon,
                s.width + rayon * 2,
                s.height + rayon * 2
            );

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
        List<RectInt> nonRelie = new List<RectInt>(salles);
        nonRelie.Remove(salleOeuf);
        nonRelie.Remove(salleDragon);

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
                    A = sA;
                    B = sB;
                    bestDist = d;
                }
            }

            CreerCouloir(Centre(A), Centre(B), false);
            arbre.Add(B);
            nonRelie.Remove(B);
        }
    }

    // ★ NOUVEAU : entrée dragon côté gauche
    Vector2Int EntreeGauche(RectInt salle)
    {
        return new Vector2Int(salle.xMin - 1, salle.yMin + salle.height / 2);
    }

    void RelierDragon()
    {
        // Liste des salles candidates (hors Dragon)
        List<RectInt> candidates = new List<RectInt>(salles);
        candidates.RemoveAll(s => RectIdentique(s, salleDragon));
        RectInt cible = candidates[Random.Range(0, candidates.Count)];

        Vector2Int entreeDragon = EntreeGauche(salleDragon);

        // ===== SEGMENT 1 : avancer 4 cases à gauche depuis l'entrée =====
        Vector2Int pointSortie = entreeDragon;
        for (int i = 0; i < 4; i++)
        {
            if (pointSortie.x <= 0) break; // sécurité bord gauche
            grille[pointSortie.x, pointSortie.y].type = Case.CaseType.Chemin;
            pointSortie.x -= 1;
        }

        // ===== SEGMENT 2 : créer un sas isolé autour du bout du couloir =====
        // On s'assure qu'il reste dans la grille
        int sasWidth = Mathf.Min(2, pointSortie.x + 1);
        int sasHeight = Mathf.Min(2, hauteur - pointSortie.y);
        RectInt sasDragon = new RectInt(pointSortie.x - sasWidth + 1, pointSortie.y, sasWidth, sasHeight);

        // On remplit le sas
        for (int x = sasDragon.xMin; x <= sasDragon.xMax; x++)
            for (int y = sasDragon.yMin; y <= sasDragon.yMax; y++)
                grille[x, y].type = Case.CaseType.Chemin;

        // Ajouter le sas à la liste des salles pour que les autres couloirs puissent s’y connecter
        salles.Add(sasDragon);

        // ===== SEGMENT 3 : créer un couloir depuis la cible jusqu'au sas =====
        CreerCouloir(Centre(cible), Centre(sasDragon), true);
    }

    void RelierOeuf()
    {
        // Liste des salles candidates (hors Oeuf)
        List<RectInt> candidates = new List<RectInt>(salles);
        candidates.RemoveAll(s => RectIdentique(s, salleOeuf));
        RectInt cible = candidates[Random.Range(0, candidates.Count)];

        Vector2Int entreeOeuf = EntreeGauche(salleOeuf);

        // ===== SEGMENT 1 : avancer 4 cases à gauche depuis l'entrée =====
        Vector2Int pointSortie = entreeOeuf;
        for (int i = 0; i < 4; i++)
        {
            if (pointSortie.x <= 0) break; // sécurité bord gauche
            grille[pointSortie.x, pointSortie.y].type = Case.CaseType.Chemin;
            pointSortie.x -= 1;
        }

        // ===== SEGMENT 2 : créer un sas isolé autour du bout du couloir =====
        // On s'assure qu'il reste dans la grille
        int sasWidth = Mathf.Min(2, pointSortie.x + 1);
        int sasHeight = Mathf.Min(2, hauteur - pointSortie.y);
        RectInt sasOeuf = new RectInt(pointSortie.x - sasWidth + 1, pointSortie.y, sasWidth, sasHeight);

        // On remplit le sas
        for (int x = sasOeuf.xMin; x <= sasOeuf.xMax; x++)
            for (int y = sasOeuf.yMin; y <= sasOeuf.yMax; y++)
                grille[x, y].type = Case.CaseType.Chemin;

        // Ajouter le sas à la liste des salles pour que les autres couloirs puissent s’y connecter
        salles.Add(sasOeuf);
        // ===== SEGMENT 3 : créer un couloir depuis la cible jusqu'au sas =====
        CreerCouloir(Centre(cible), Centre(sasOeuf), true);
    }

    Vector2Int Centre(RectInt s)
    {
        return new Vector2Int(s.xMin + s.width / 2, s.yMin + s.height / 2);
    }

    void CreerCouloir(Vector2Int a, Vector2Int b, bool forcer)
    {
        int x = a.x;
        int y = a.y;

        while (x != b.x)
        {
            grille[x, y].type = Case.CaseType.Chemin;
            x += (b.x > x) ? 1 : -1;
        }

        while (y != b.y)
        {
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

            foreach (Vector2Int v in new Vector2Int[]
            {
                p + Vector2Int.up,
                p + Vector2Int.down,
                p + Vector2Int.left,
                p + Vector2Int.right
            })
            {
                if (v.x >= 0 && v.x < largeur && v.y >= 0 && v.y < hauteur)
                    if (grille[v.x, v.y].type == Case.CaseType.Chemin)
                        file.Enqueue(v);
            }
        }
        return false;
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
