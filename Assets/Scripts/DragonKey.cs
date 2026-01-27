using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonKey : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            inv.hasDragonKey = true;
            Destroy(gameObject);
        }
    }
}
