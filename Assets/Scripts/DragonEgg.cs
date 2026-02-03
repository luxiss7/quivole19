using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonEgg : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // on vérifie juste que c’est un joueur
        if (!other.GetComponent<Player>()) return;
        GameState.Instance.dragonEggRecupere = true;

        Destroy(gameObject);
    }
}
