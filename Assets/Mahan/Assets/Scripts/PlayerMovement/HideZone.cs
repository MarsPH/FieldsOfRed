using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HideZone : MonoBehaviour
{
    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHiding playerHiding = other.GetComponent<PlayerHiding>();
        if (playerHiding == null)
            return;

        playerHiding.EnterHideZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerHiding playerHiding = other.GetComponent<PlayerHiding>();
        if (playerHiding == null)
            return;

        playerHiding.ExitHideZone(this);
    }
}