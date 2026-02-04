using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject InfoPanel;

    [Header("Boutons du menu")]
    public List<Button> boutons; 
    // 0 = Play, 1 = Infos, 2 = Quit

    private int indexSelection = 0;

    [Header("Couleurs")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.cyan;

    void Start()
    {
        InfoPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        MettreAJourSelection();
    }

    void OnEnable()
    {
        RFIDEventManager.OnRFIDDetected += OnRFIDDetected;
    }

    void OnDisable()
    {
        RFIDEventManager.OnRFIDDetected -= OnRFIDDetected;
    }

    void Update()
    {
        if (!mainMenuPanel.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.W))
            DeplacerSelection(-1);

        if (Input.GetKeyDown(KeyCode.S))
            DeplacerSelection(1);

        if (Input.GetKeyDown(KeyCode.D))
            Valider();
    }

    void OnRFIDDetected(int lecteur, string role)
    {
        if (!mainMenuPanel.activeSelf)
            return;

        switch (lecteur)
        {
            case 1:
                // Capteur 1 = W (monter)
                DeplacerSelection(-1);
                break;
            case 3:
                // Capteur 3 = S (descendre)
                DeplacerSelection(1);
                break;
            case 2:
            case 4:
                // Capteur 2 ou 4 = D (valider)
                Valider();
                break;
            default:
                Debug.LogWarning("Lecteur RFID inconnu : " + lecteur);
                break;
        }
    }

    void DeplacerSelection(int direction)
    {
        indexSelection += direction;

        if (indexSelection < 0)
            indexSelection = boutons.Count - 1;
        else if (indexSelection >= boutons.Count)
            indexSelection = 0;

        MettreAJourSelection();
    }

    void MettreAJourSelection()
    {
        for (int i = 0; i < boutons.Count; i++)
        {
            Image img = boutons[i].GetComponent<Image>();
            img.color = (i == indexSelection) ? selectedColor : normalColor;
        }
    }

    void Valider()
    {
        switch (indexSelection)
        {
            case 0:
                Jouer();
                break;

            case 1:
                OuvrirInfos();
                break;

            case 2:
                Quitter();
                break;
        }
    }

    void Jouer()
    {
        SceneManager.LoadScene("IntroScene");
    }

    void OuvrirInfos()
    {
        mainMenuPanel.SetActive(false);
        InfoPanel.SetActive(true);
    }

    void Quitter()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }
}
