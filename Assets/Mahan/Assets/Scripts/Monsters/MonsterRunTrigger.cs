using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MonsterRunTrigger : MonoBehaviour
{
    public ManMonster monster;
    public Transform targetPoint;
    public bool triggerOnlyOnce = true;

    private bool used;

    void Reset()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered trigger: " + other.name);

        if (used && triggerOnlyOnce)
            return;

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Entered object is not tagged Player");
            return;
        }

        if (monster == null)
        {
            Debug.LogError("Monster reference is missing");
            return;
        }

        if (targetPoint == null)
        {
            Debug.LogError("Target point reference is missing");
            return;
        }

        Debug.Log("Trigger fired. Sending monster to: " + targetPoint.position);
        monster.TriggerRunToPointAndVanish(targetPoint);

        used = true;

        if (triggerOnlyOnce)
            gameObject.SetActive(false);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        BoxCollider box = GetComponent<BoxCollider>();

        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }

        if (targetPoint != null)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, targetPoint.position);
        }
    }
}