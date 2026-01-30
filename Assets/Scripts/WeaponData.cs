using UnityEngine;

[CreateAssetMenu(menuName = "Jeu/Weapon")]
public class WeaponData : ScriptableObject
{
    public string nom;
    public int degats;
    public WeaponType type;
    public Sprite sprite;
    public GameObject prefab;

    public enum WeaponType
    {
        Melee,
        Distance
    }
}