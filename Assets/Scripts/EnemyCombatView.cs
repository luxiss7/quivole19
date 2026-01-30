using UnityEngine;

public class EnemyCombatView : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public void Init(EnemyCombatant enemy)
    {
        spriteRenderer.sprite = enemy.data.sprite;
    }
}