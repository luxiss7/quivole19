using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EndMenuUIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;

    [Header("Boutons du menu")]
    public List<Button> boutons; 

    private int indexSelection = 0;

    [Header("Couleurs")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.cyan;

    void Start()
    {
        mainMenuPanel.SetActive(true);

        MettreAJourSelection();
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
                EcranTitre();
                break;

            case 2:
                Quitter();
                break;
        }
    }

    void Jouer()
    {
        SceneManager.LoadScene("MainDonjon");
    }

    void EcranTitre()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void Quitter()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }
}
