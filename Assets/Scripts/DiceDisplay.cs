using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Affiche le résultat du dé au centre de l'écran
/// Pour les combats ET les déplacements dans le donjon
/// </summary>
public class DiceDisplay : MonoBehaviour
{
    public static DiceDisplay Instance;

    [Header("UI du dé")]
    public GameObject dicePanel;        // Panel qui contient l'affichage
    public Text diceText;               // Texte qui affiche le nombre
    public Text labelText;              // Texte qui affiche le contexte (ex: "Dé d'attaque")
    
    [Header("Paramètres d'affichage")]
    public float displayDuration = 1.5f; // Durée d'affichage en secondes
    public float fadeSpeed = 2f;         // Vitesse du fade in/out

    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Récupérer le CanvasGroup pour les animations
        if (dicePanel != null)
        {
            canvasGroup = dicePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = dicePanel.AddComponent<CanvasGroup>();
            }
        }
    }

    void Start()
    {
        // Cacher le panel au démarrage
        if (dicePanel != null)
        {
            dicePanel.SetActive(false);
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Afficher le résultat d'un dé de déplacement
    /// </summary>
    public void AfficherDeDeplacement(int valeur)
    {
        AfficherDe(valeur, "Déplacement", Color.white);
    }

    /// <summary>
    /// Afficher le résultat d'un dé d'attaque
    /// </summary>
    public void AfficherDeAttaque(int valeur, bool critique = false)
    {
        Color couleur = critique ? new Color(1f, 0.8f, 0f) : Color.red; // Jaune si critique, rouge sinon
        string label = critique ? "⭐ CRITIQUE !" : "Attaque";
        AfficherDe(valeur, label, couleur);
    }

    /// <summary>
    /// Afficher le résultat d'un dé de défense
    /// </summary>
    public void AfficherDeDefense(int valeur, bool parfait = false)
    {
        Color couleur = parfait ? new Color(0f, 1f, 0.5f) : Color.blue;
        string label = parfait ? "⭐ DÉFENSE PARFAITE !" : "Défense";
        AfficherDe(valeur, label, couleur);
    }

    /// <summary>
    /// Afficher le résultat d'un dé de soin
    /// </summary>
    public void AfficherDeSoin(int valeur, bool critique = false)
    {
        Color couleur = critique ? new Color(0f, 1f, 0.5f) : Color.green;
        string label = critique ? "⭐ SOIN CRITIQUE !" : "Soin";
        AfficherDe(valeur, label, couleur);
    }

    /// <summary>
    /// Afficher le résultat d'un dé ennemi
    /// </summary>
    public void AfficherDeEnnemi(int valeur, bool critique = false)
    {
        Color couleur = critique ? new Color(0.5f, 0f, 0f) : new Color(0.8f, 0.2f, 0.2f);
        string label = critique ? "⭐ ENNEMI CRITIQUE !" : "Ennemi";
        AfficherDe(valeur, label, couleur);
    }

    /// <summary>
    /// Afficher un dé pour un joueur KO (toujours 0)
    /// </summary>
    public void AfficherDeKO(string nomJoueur)
    {
        AfficherDe(0, $"💀 {nomJoueur} KO", Color.gray);
    }

        /// <summary>
    /// Méthode générique pour afficher un dé
    /// </summary>
    private void AfficherDe(int valeur, string label, Color couleur)
    {
        if (dicePanel == null || diceText == null)
        {
            Debug.LogError("[DiceDisplay] UI non configurée !");
            return;
        }

        dicePanel.SetActive(true);  
        StopAllCoroutines(); // Arrêter toute animation en cours
        StartCoroutine(AfficherCoroutine(valeur, label, couleur));
    }

    private IEnumerator AfficherCoroutine(int valeur, string label, Color couleur)
    {
        // Configurer les textes
        diceText.text = valeur.ToString();
        diceText.color = couleur;

        if (labelText != null)
        {
            labelText.text = label;
            labelText.color = couleur;
        }

        // Activer le panel
        //dicePanel.SetActive(true);

        // Fade in
        if (canvasGroup != null)
        {
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * fadeSpeed;
                canvasGroup.alpha = alpha;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // Attendre
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        if (canvasGroup != null)
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * fadeSpeed;
                canvasGroup.alpha = alpha;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        // Désactiver le panel
        dicePanel.SetActive(false);
    }

    /// <summary>
    /// Cacher immédiatement le dé (utile pour nettoyer l'écran)
    /// </summary>
    public void Cacher()
    {
        StopAllCoroutines();
        if (dicePanel != null)
            dicePanel.SetActive(false);
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
}
