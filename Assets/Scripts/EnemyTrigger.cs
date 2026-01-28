using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    public EnemyData enemyData;
    bool combatDeclenche = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (combatDeclenche) return;

        Player p = other.GetComponent<Player>();
        if (!p) return;

        combatDeclenche = true;

        CombatManager.Instance.StartCombat(gameObject, enemyData);
    }
}
