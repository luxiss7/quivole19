using UnityEngine;

public class ColorCubeManager : MonoBehaviour
{
    [Header("Materials")]
    public Material red;
    public Material blue;
    public Material green;
    public Material yellow;
    public Material orange;
    public Material white;

    Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        ColorEventManager.OnColorDetected += OnColorDetected;
    }

    void OnDisable()
    {
        ColorEventManager.OnColorDetected -= OnColorDetected;
    }

    void OnColorDetected(string color, int value)
    {
        Debug.Log("Cube change → " + color);

        switch (color)
        {
            case "RED":
                rend.material = red;
                break;
            case "BLUE":
                rend.material = blue;
                break;
            case "GREEN":
                rend.material = green;
                break;
            case "YELLOW":
                rend.material = yellow;
                break;
            case "ORANGE":
                rend.material = orange;
                break;
            case "WHITE":
                rend.material = white;
                break;
            default:
                Debug.LogWarning("Couleur inconnue: " + color);
                break;
        }
    }
}
