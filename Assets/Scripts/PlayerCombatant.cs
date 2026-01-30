[System.Serializable]
public class PlayerCombatant
{
    public PlayerData data;
    public int hpActuels;
    public bool estVivant = true;

    public PlayerCombatant(PlayerData d)
    {
        data = d;
        hpActuels = d.pointsDeVie;
    }
}