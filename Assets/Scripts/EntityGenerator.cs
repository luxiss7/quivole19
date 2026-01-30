using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGenerator : MonoBehaviour
{
    public DonjonGenerator donjon;

    [Header("Objets")]
    public GameObject dragonDoorPrefab;
    public GameObject dragonKeyPrefab;

    public List<GameObject> weaponPrefabs;

    [Header("Enemies")]
    public GameObject dragonPrefab;
    public GameObject guardianPrefab;
    public List<GameObject> enemyPrefabs;

    private Case[,] grille;
    private RectInt salleSpawn;
    private RectInt salleOeuf;
    private RectInt salleDragon;
    private Vector2Int offset;

    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return null;

        grille = donjon.GetGrille();
        salleSpawn = donjon.GetSalleSpawn();
        salleOeuf = donjon.GetSalleOeuf();
        salleDragon = donjon.GetSalleDragon();
        offset = donjon.GetOffset();

        if (GameState.Instance.entitesGenerees)
        {
            RestaurerEntites();
            Debug.Log("[Entity] Restauration");
        }
        else
        {
            GenererEntites();
            SauvegarderEntites();
            GameState.Instance.entitesGenerees = true;
            Debug.Log("[Entity] Génération initiale");
        }
    }

    // ==========================================================
    // GÉNÉRATION
    // ==========================================================
    void GenererEntites()
    {
        // Porte
        Vector2Int posPorte = TrouverJonction(salleDragon);
        GameState.Instance.positionPorteDragon = posPorte;

        // Clé
        Vector2Int posCle = TrouverCleDragon();
        GameState.Instance.positionCleDragon = posCle;

        // Ennemis fixes
        AjouterEnnemi(dragonPrefab, Centre(salleDragon));
        AjouterEnnemi(guardianPrefab, Centre(salleOeuf));

        // Ennemis aléatoires
        SpawnRandomEnemies();

        // Armes au sol
        SpawnRandomWeapons();
    }

    void AjouterEnnemi(GameObject prefab, Vector2Int pos)
    {
        GameState.Instance.ennemisDonjon.Add(new EntityData
        {
            prefab = prefab,
            position = pos,
            estMort = false
        });
    }

    // ==========================================================
    // RESTAURATION
    // ==========================================================
    void RestaurerEntites()
    {
        // Porte
        Instantiate(dragonDoorPrefab, Monde(GameState.Instance.positionPorteDragon), Quaternion.identity);

        // Clé
        Instantiate(dragonKeyPrefab, Monde(GameState.Instance.positionCleDragon), Quaternion.identity);

        // Ennemis
        foreach (var e in GameState.Instance.ennemisDonjon)
        {
            if (e.estMort) continue;
            Instantiate(e.prefab, Monde(e.position), Quaternion.identity);
        }

        // Armes
        foreach (var a in GameState.Instance.armesDonjon)
        {
            if (a.ramassee) continue;
            Instantiate(a.prefab, Monde(a.position), Quaternion.identity);
        }
    }

    void SauvegarderEntites()
    {
        // juste forcer la restauration immédiate
        RestaurerEntites();
    }

    // ==========================================================
    // HELPERS
    // ==========================================================
    Vector3 Monde(Vector2Int p)
        => new Vector3(p.x + offset.x + 0.5f, p.y + offset.y + 0.5f, 0);

    Vector2Int Centre(RectInt s)
        => new Vector2Int(s.xMin + s.width / 2, s.yMin + s.height / 2);

    Vector2Int TrouverCleDragon()
    {
        List<Vector2Int> candidats = new();
        for (int x = salleOeuf.xMin; x <= salleOeuf.xMax; x++)
        for (int y = salleOeuf.yMin; y <= salleOeuf.yMax; y++)
            if (grille[x, y].type == Case.CaseType.Chemin)
                candidats.Add(new Vector2Int(x, y));

        return candidats[Random.Range(0, candidats.Count)];
    }

    Vector2Int TrouverJonction(RectInt salle)
    {
        for (int x = salle.xMin; x <= salle.xMax; x++)
        for (int y = salle.yMin; y <= salle.yMax; y++)
        {
            Vector2Int p = new(x, y);
            if (grille[p.x, p.y].type != Case.CaseType.Chemin) continue;

            foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int v = p + d;
                if (!salle.Contains(v) && grille[v.x, v.y].type == Case.CaseType.Chemin)
                    return v;
            }
        }
        return Centre(salle);
    }

    void SpawnRandomEnemies()
    {
        int nombreAPlacer = Random.Range(4, 13); // nombre d'ennemis aléatoire
        List<Vector2Int> positionsValides = new List<Vector2Int>();

        // Parcours toutes les cases du donjon
        for (int x = 0; x < grille.GetLength(0); x++)
        {
            for (int y = 0; y < grille.GetLength(1); y++)
            {
                if (grille[x, y].type != Case.CaseType.Chemin)
                    continue;

                Vector2Int p = new Vector2Int(x, y);

                // Exclure les salles spéciales
                if (salleSpawn.Contains(p)) continue;
                if (salleOeuf.Contains(p)) continue;
                if (salleDragon.Contains(p)) continue;

                positionsValides.Add(p);
            }
        }

        if (positionsValides.Count == 0 || enemyPrefabs.Count == 0)
            return;

        // Mélanger les positions valides
        for (int i = 0; i < positionsValides.Count; i++)
        {
            int r = Random.Range(i, positionsValides.Count);
            (positionsValides[i], positionsValides[r]) = (positionsValides[r], positionsValides[i]);
        }

        // Mélanger la liste des ennemis dispo
        List<GameObject> ennemisDispos = new List<GameObject>(enemyPrefabs);
        for (int i = 0; i < ennemisDispos.Count; i++)
        {
            int r = Random.Range(i, ennemisDispos.Count);
            (ennemisDispos[i], ennemisDispos[r]) = (ennemisDispos[r], ennemisDispos[i]);
        }

        int count = Mathf.Min(nombreAPlacer, positionsValides.Count, ennemisDispos.Count);

        for (int i = 0; i < count; i++)
        {
            Vector2Int p = positionsValides[i];
            Vector3 posMonde = new Vector3(p.x + offset.x + 0.5f,
                                        p.y + offset.y + 0.5f,
                                        0);
            Instantiate(ennemisDispos[i], posMonde, Quaternion.identity);
        }
    }

    void SpawnRandomWeapons()
    {
        int nombreAPlacer = Random.Range(7, 12); // nombre d'armes aléatoire
        List<Vector2Int> positionsValides = new List<Vector2Int>();

        // Parcours toutes les cases du donjon
        for (int x = 0; x < grille.GetLength(0); x++)
        {
            for (int y = 0; y < grille.GetLength(1); y++)
            {
                if (grille[x, y].type != Case.CaseType.Chemin)
                    continue;

                Vector2Int p = new Vector2Int(x, y);

                // Exclure les salles spéciales
                if (salleSpawn.Contains(p)) continue;
                if (salleOeuf.Contains(p)) continue;
                if (salleDragon.Contains(p)) continue;

                positionsValides.Add(p);
            }
        }

        if (positionsValides.Count == 0 || weaponPrefabs.Count == 0)
            return;

        // Mélanger les positions valides
        for (int i = 0; i < positionsValides.Count; i++)
        {
            int r = Random.Range(i, positionsValides.Count);
            (positionsValides[i], positionsValides[r]) = (positionsValides[r], positionsValides[i]);
        }

        // Mélanger la liste des armes dispo
        List<GameObject> armesDispos = new List<GameObject>(weaponPrefabs);
        for (int i = 0; i < armesDispos.Count; i++)
        {
            int r = Random.Range(i, armesDispos.Count);
            (armesDispos[i], armesDispos[r]) = (armesDispos[r], armesDispos[i]);
        }

        int count = Mathf.Min(nombreAPlacer, positionsValides.Count, armesDispos.Count);

        for (int i = 0; i < count; i++)
        {
            Vector2Int p = positionsValides[i];
            Vector3 posMonde = new Vector3(p.x + offset.x + 0.5f,
                                        p.y + offset.y + 0.5f,
                                        0);
            Instantiate(armesDispos[i], posMonde, Quaternion.identity);
        }
    }
}
