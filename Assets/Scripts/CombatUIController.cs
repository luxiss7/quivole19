using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ContrÃ´le les boutons d'action du combat avec les touches Z/S/D
/// VERSION COMPATIBLE avec CombatManager_SIMPLE
/// </summary>
public class CombatUIController : MonoBehaviour
{
    public CombatManager combatManager;

    [Header("Boutons")]
    public List<Button> boutons; // 0=Attack, 1=Defense, 2=Heal

    private int indexSelection = 0;

    [Header("Couleurs")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    void Start()
    {
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
        if (combatManager == null)
            return;

        // Navigation avec W/S
        if (Input.GetKeyDown(KeyCode.W))
            DeplacerSelection(-1);

        if (Input.GetKeyDown(KeyCode.S))
            DeplacerSelection(1);

        // Validation avec D
        if (Input.GetKeyDown(KeyCode.D))
            Valider();
    }

    void OnRFIDDetected(int lecteur, string role)
    {
        if (combatManager == null || !combatManager.combatEnCours)
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
            if (img != null)
                img.color = (i == indexSelection) ? selectedColor : normalColor;
        }
    }

    void Valider()
    {
        switch (indexSelection)
        {
            case 0:
                combatManager.BoutonAttaquer();
                break;

            case 1:
                combatManager.BoutonDefendre();
                break;

            case 2:
                combatManager.BoutonSoigner();
                break;
        }
    }
}