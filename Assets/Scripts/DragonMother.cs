using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonMother : MonoBehaviour
{
    public bool isFriendly = false;
    public Sprite angrySprite;
    public Sprite friendlySprite;

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
        // si l'oeuf vient d’être récupérée, on ouvre
        if (!isFriendly && GameState.Instance.dragonEggRecupere)
        {
            Friendly();
        }
    }

    void Friendly()
    {
        isFriendly = true;
        sr.sprite = friendlySprite;
    }

    void UpdateState()
    {
        if (GameState.Instance.dragonEggRecupere)
            Friendly();
        else
            sr.sprite = angrySprite;
    }
}