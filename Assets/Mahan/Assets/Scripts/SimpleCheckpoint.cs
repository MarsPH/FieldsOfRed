using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleCheckpoint : MonoBehaviour
{
    public static bool checkpointReached = false;

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool used;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used && triggerOnlyOnce)
            return;

        if (!other.CompareTag(playerTag))
            return;

        checkpointReached = true;
        used = true;

        ManMonster[] monsters = FindObjectsOfType<ManMonster>();

        for (int i = 0; i < monsters.Length; i++)
        {
            Destroy(monsters[i].gameObject);
        }
    }
}