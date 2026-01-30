using UnityEngine;

[CreateAssetMenu(fileName = "NouvelleClasse", menuName = "Jeu/Classe")]
public class ClasseData : ScriptableObject
{
    public string nomClasse;

    [Header("Stats")]
    public int pointsDeVie;
    public int melee;
    public int distance;
    public int crochetage;

    [Header("Apparence")]
    public Sprite sprite;

    [Header("Armes de départ")]
    public WeaponData arme1;
    public WeaponData arme2;
}
