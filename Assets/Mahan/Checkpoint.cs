using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private bool activateOnlyOnce = true;

    private bool activated;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (useTrigger)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger)
            return;

        TryActivate(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useTrigger)
            return;

        TryActivate(collision.gameObject);
    }

    private void TryActivate(GameObject other)
    {
        if (activated && activateOnlyOnce)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (RespawnManager.Instance == null)
            return;

        RespawnManager.Instance.SetCheckpoint(transform);
        activated = true;
    }
}