using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatUIController : MonoBehaviour
{
    public CombatManager combatManager;

    [Header("Boutons")]
    public List<Button> boutons; // 0=Attack,1=Defense,2=Heal,3=Items

    private int indexSelection = 0;

    [Header("Couleurs")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    void Start()
    {
        MettreAJourSelection();
    }

    void Update()
    {
        if (combatManager == null)
            return;

        if (combatManager.state != CombatManager.CombatState.PlayerTurn)
            return;

        if (Input.GetKeyDown(KeyCode.Z))
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
                combatManager.PlayerAttack();
                break;

            case 1:
                Debug.Log("DEFENSE (à implémenter)");
                break;

            case 2:
                Debug.Log("HEAL (à implémenter)");
                break;

            case 3:
                Debug.Log("ITEMS (à implémenter)");
                break;
        }
    }
}
