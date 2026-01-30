using UnityEngine;

public class WeaponTrigger : MonoBehaviour
{
    public WeaponData weaponData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameManager.Instance.weaponPickupAutorise)
            return;

        Player player = other.GetComponent<Player>();
        if (player == null) return;

        GameManager.Instance.weaponPickupAutorise = false;
        WeaponPickupUI.Instance.OuvrirMenu(player, this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;
    }
}
