using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    public CombatManager combat;
    public Slider enemyHP;

    public Slider[] playerHP;

    void Update()
    {
        enemyHP.maxValue = combat.ennemi.data.pointsDeVie;
        enemyHP.value = combat.ennemi.hpActuels;

        for (int i = 0; i < combat.joueurs.Count; i++)
        {
            playerHP[i].maxValue = combat.joueurs[i].data.pointsDeVie;
            playerHP[i].value = combat.joueurs[i].hpActuels;
        }
    }
}
