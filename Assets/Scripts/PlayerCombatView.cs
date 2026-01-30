using UnityEngine;

public class PlayerCombatView : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public void Init(PlayerCombatant combatant)
    {
        spriteRenderer.sprite = combatant.data.classeData.sprite;
    }
}