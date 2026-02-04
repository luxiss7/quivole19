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
        RFIDEventManager.OnRFIDDetected += OnRFIDDetected;
    }

    void OnDisable()
    {
        RFIDEventManager.OnRFIDDetected -= OnRFIDDetected;
    }

    void Update()
    {
        if (!infoPanel.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.D))
            FermerInfos();
    }

    void OnRFIDDetected(int lecteur, string role)
    {
        if (!infoPanel.activeSelf)
            return;

        // Readers 2 or 4 act like 'D' (validate)
        if (lecteur == 2 || lecteur == 4)
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
