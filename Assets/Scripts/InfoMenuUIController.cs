using UnityEngine;
using UnityEngine.UI;

public class InfoMenuUIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject infoPanel;
    public GameObject mainMenuPanel;

    [Header("Bouton Retour")]
    public Button retourButton;

    [Header("Couleurs")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.cyan;

    void OnEnable()
    {
        MettreAJourSelection();
    }

    void Update()
    {
        if (!infoPanel.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.D))
            FermerInfos();
    }

    void MettreAJourSelection()
    {
        Image img = retourButton.GetComponent<Image>();
        img.color = selectedColor;
    }

    void FermerInfos()
    {
        infoPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
