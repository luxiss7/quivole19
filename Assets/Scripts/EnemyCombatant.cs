[System.Serializable]
public class EnemyCombatant
{
    public EnemyData data;
    public int hpActuels;
    public bool estVivant = true;

    public EnemyCombatant(EnemyData d)
    {
        data = d;
        hpActuels = d.pointsDeVie;
    }
}
