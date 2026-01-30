using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonDoor : MonoBehaviour
{
    public bool isOpen = false;
    public Sprite closedSprite;
    public Sprite openSprite;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // état initial
        UpdateState();
    }

    void Update()
    {
        // si la clé vient d’être récupérée, on ouvre
        if (!isOpen && GameState.Instance.dragonKeyRecuperee)
        {
            Open();
        }
    }

    void Open()
    {
        isOpen = true;
        sr.sprite = openSprite;
    }

    void UpdateState()
    {
        if (GameState.Instance.dragonKeyRecuperee)
            Open();
        else
            sr.sprite = closedSprite;
    }
}