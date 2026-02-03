using UnityEngine;

public class CubesManager : MonoBehaviour
{
    [SerializeField] private GameObject cube_rouge;
    [SerializeField] private GameObject cube_vert;
    [SerializeField] private GameObject cube_bleu;
    [SerializeField] private GameObject cube_noir;

    void Start()
    {
        
    }

    void OnEnable()
    {
        RFIDEventManager.OnRFIDDetected += OnRFIDDetected;
        ColorEventManager.OnColorDetected += OnDiceRolled;
    }

    void OnDisable()
    {
        RFIDEventManager.OnRFIDDetected -= OnRFIDDetected;
        ColorEventManager.OnColorDetected -= OnDiceRolled;
    }

    void OnRFIDDetected(int lecteur, string role)
    {
        switch (role)
        {
            case "Archer":
                if (lecteur == 1)
                {
                    MoveForward(cube_rouge);
                }
                else if (lecteur == 2)
                {
                    MoveLeft(cube_rouge);
                }
                else if (lecteur == 3)
                {
                    MoveBackward(cube_rouge);
                }
                else if (lecteur == 4)
                {
                    MoveRight(cube_rouge);
                }
                break;

            case "Voleur":
                if (lecteur == 1)
                {
                    MoveForward(cube_vert);
                }
                else if (lecteur == 2)
                {
                    MoveLeft(cube_vert);
                }
                else if (lecteur == 3)
                {
                    MoveBackward(cube_vert);
                }
                else if (lecteur == 4)
                {
                    MoveRight(cube_vert);
                }
                break;

            case "Tank":
                if (lecteur == 1)
                {
                    MoveForward(cube_bleu);
                }
                else if (lecteur == 2)
                {
                    MoveLeft(cube_bleu);
                }
                else if (lecteur == 3)
                {
                    MoveBackward(cube_bleu);
                }
                else if (lecteur == 4)
                {
                    MoveRight(cube_bleu);
                }
                break;

            case "soigneur":
                if (lecteur == 1)
                {
                    MoveForward(cube_noir);
                }
                else if (lecteur == 2)
                {
                    MoveLeft(cube_noir);
                }
                else if (lecteur == 3)
                {
                    MoveBackward(cube_noir);
                }
                else if (lecteur == 4)
                {
                    MoveRight(cube_noir);
                }
                break;
        }
    }

    void MoveForward(GameObject cube)
    {
        cube.transform.position = cube.transform.position + Vector3.forward * 1.0f;
    }

    void MoveLeft(GameObject cube)
    {
        cube.transform.position = cube.transform.position + Vector3.left * 1.0f;
    }

    void MoveBackward(GameObject cube)
    {
        cube.transform.position = cube.transform.position + Vector3.back * 1.0f;
    }

    void MoveRight(GameObject cube)
    {
        cube.transform.position = cube.transform.position + Vector3.right * 1.0f;
    }

    void OnDiceRolled(string color, int value)
    {
        Debug.Log("Dé lancé → " + color + " = " + value);
        // Exemple :
        // player.Move(value);
    }
}
