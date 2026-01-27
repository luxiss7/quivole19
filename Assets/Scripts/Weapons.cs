using UnityEngine;

[System.Serializable]
public class Weapon
{
    public string nom;
    public int degats;
    public WeaponType type;

    public enum WeaponType { Melee, Distance }
}
