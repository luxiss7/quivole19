using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class IntroUIController : MonoBehaviour
{
    [Header("Pages du livre")]
    public List<GameObject> pages; 
    // 0 = Page1, 1 = Page2, ... 4 = Page5

    private int indexPage = 0;

    [Header("Boutons d'action")]
    public List<Button> actionButtons;
    // 0 = Page précédente, 1 = Page suivante, 2 = Skip

    private int indexSelection = 0;

    [Header("Couleurs")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.cyan;

    void Start()
    {
        AfficherPage(indexPage);
        MettreAJourSelection();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            DeplacerSelection(-1);

        if (Input.GetKeyDown(KeyCode.S))
            DeplacerSelection(1);

        if (Input.GetKeyDown(KeyCode.D))
            Valider();
    }

    // --------------------
    // Navigation boutons
    // --------------------

    void DeplacerSelection(int direction)
    {
        indexSelection += direction;

        if (indexSelection < 0)
            indexSelection = actionButtons.Count - 1;
        else if (indexSelection >= actionButtons.Count)
            indexSelection = 0;

        MettreAJourSelection();
    }

    void MettreAJourSelection()
    {
        for (int i = 0; i < actionButtons.Count; i++)
        {
            Image img = actionButtons[i].GetComponent<Image>();
            img.color = (i == indexSelection) ? selectedColor : normalColor;
        }
    }

    void Valider()
    {
        switch (indexSelection)
        {
            case 0:
                PagePrecedente();
                break;

            case 1:
                PageSuivante();
                break;

            case 2:
                SkipIntro();
                break;
        }
    }

    // --------------------
    // Gestion des pages
    // --------------------

    void AfficherPage(int index)
    {
        for (int i = 0; i < pages.Count; i++)
            pages[i].SetActive(i == index);
    }

    void PageSuivante()
    {
        if (indexPage < pages.Count - 1)
        {
            indexPage++;
            AfficherPage(indexPage);
        }
    }

    void PagePrecedente()
    {
        if (indexPage > 0)
        {
            indexPage--;
            AfficherPage(indexPage);
        }
    }

    // --------------------
    // Skip
    // --------------------

    void SkipIntro()
    {
        SceneManager.LoadScene("MainDonjon");
    }
}

// test 
