using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonDoor : MonoBehaviour
{
    public bool isOpen = false;
    public Sprite closedSprite;
    public Sprite openSprite;

    private SpriteRenderer sr;
    private BoxCollider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        SetState(false);
    }

    public void TryOpen(bool playerHasKey)
    {
        if (!playerHasKey) return;
        SetState(true);
    }

    void SetState(bool open)
    {
        isOpen = open;
        sr.sprite = open ? openSprite : closedSprite;
        col.isTrigger = open; // fermé = blocage, ouvert = traversable
    }
}
