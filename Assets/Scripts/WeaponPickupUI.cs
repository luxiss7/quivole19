using UnityEngine;
using UnityEngine.UI;
using System;

public class WeaponPickupUI : MonoBehaviour
{
    public static WeaponPickupUI Instance;

    [Header("UI")]
    public GameObject weaponChoiceUI; // le menu visuel
    public Image armeRamasseeImage;
    public Image arme1Image;
    public Image arme2Image;

    private Player player;
    private WeaponTrigger armeAuSol;
    private bool menuActif = false;
    public static event Action OnMenuFerme;
    public bool MenuEstOuvert => menuActif;


    void Awake()
    {
        Instance = this;
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
        if (!menuActif) return;

        if (Input.GetKeyDown(KeyCode.A))
            ChoisirSlot1();
        if (Input.GetKeyDown(KeyCode.D))
            ChoisirSlot2();
        if (Input.GetKeyDown(KeyCode.W))
            Refuser();
    }

    void OnRFIDDetected(int lecteur, string role)
    {
        // Vérifier que c'est le joueur actif
        Player playerActif = gameManager.joueurs[gameManager.tourActuel];
        if (playerActif != GetComponent<Player>())
            return; // Pas le joueur actif → ignorer

        // Vérifier que le rôle RFID correspond au joueur actif
        if (role.ToLower() != playerActif.classeData.nomClasse.ToLower())
            return; // mauvais RFID → ignorer

        if (!menuActif) return;

        switch (lecteur)
        {
            case 1: // Reader 1 = Refuse
                Refuser();
                break;
            case 2: // Reader 2 = Slot 1
                ChoisirSlot1();
                break;
            case 3:
            case 4: // Reader 4 = Slot 2
                ChoisirSlot2();
                break;
            default:
                Debug.LogWarning("Lecteur RFID inconnu : " + lecteur);
                break;
        }
    }

    public void OuvrirMenu(Player p, WeaponTrigger arme)
    {
        player = p;
        armeAuSol = arme;
        menuActif = true;

        Time.timeScale = 0f; // pause
        weaponChoiceUI.SetActive(true); // seulement le menu visuel
        RafraichirUI();
    }

    void RafraichirUI()
    {
        // Arme ramassée
        armeRamasseeImage.sprite = armeAuSol.weaponData.sprite;

        // Slot 1
        arme1Image.sprite = player.arme1 != null ? player.arme1.sprite : null;
        arme1Image.enabled = player.arme1 != null;

        // Slot 2
        arme2Image.sprite = player.arme2 != null ? player.arme2.sprite : null;
        arme2Image.enabled = player.arme2 != null;
    }

    void ChoisirSlot1() => Echanger(ref player.arme1);
    void ChoisirSlot2() => Echanger(ref player.arme2);
    void Refuser() => FermerMenu();

    void Echanger(ref WeaponData slot)
    {
        WeaponData nouvelleArme = armeAuSol.weaponData;
        WeaponData ancienneArme = slot;

        slot = nouvelleArme;

        if (ancienneArme != null)
        {
            GameObject go = Instantiate(
                ancienneArme.prefab,
                armeAuSol.transform.position,
                Quaternion.identity
            );
        }

        Destroy(armeAuSol.gameObject);
        FermerMenu();
    }

    void FermerMenu()
    {
        menuActif = false;
        Time.timeScale = 1f;
        weaponChoiceUI.SetActive(false);
        OnMenuFerme?.Invoke();
    }
}