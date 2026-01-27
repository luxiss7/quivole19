using UnityEngine;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    [Header("Donjon")]
    public bool donjonGenere = false;
    public DonjonData donjonData;

    [Header("Joueurs")]
    public List<PlayerData> joueurs = new();

    [Header("Entités Donjon")]
    public bool entitesGenerees = false;
    public List<EntityData> ennemisDonjon = new();
    public Vector2Int positionCleDragon;
    public Vector2Int positionPorteDragon;

    [Header("Combat")]
    public List<PlayerData> party = new();
    public EnemyData enemyData;

    [Header("Scenes")]
    public string sceneRetour = "SavedDonjon";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ClearCombatData()
    {
        party.Clear();
        enemyData = null;
    }
}
