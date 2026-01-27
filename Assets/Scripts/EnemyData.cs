using UnityEngine;

[CreateAssetMenu(menuName = "Jeu/Ennemi")]
public class EnemyData : ScriptableObject
{
    public string nom;

    [Header("Stats")]
    public int pointsDeVie;
    public int degats;

    [Header("Apparence")]
    public Sprite sprite;
}
