using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform cible;           // La cible actuelle (le joueur du tour)
    public float vitesse = 3f;        // Plus c’est grand, plus ça suit vite
    public Vector3 offset = new Vector3(0, 0, -10);  // Décalage standard caméra 2D

    void LateUpdate()
    {
        if (cible == null) return;

        Vector3 positionFinale = cible.position + offset;
        transform.position = Vector3.Lerp(transform.position, positionFinale, Time.deltaTime * vitesse);
    }
}
