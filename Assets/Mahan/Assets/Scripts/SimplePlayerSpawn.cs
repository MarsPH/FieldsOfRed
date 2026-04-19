using UnityEngine;

public class SimplePlayerSpawn : MonoBehaviour
{
    [SerializeField] private Transform initialSpawn;
    [SerializeField] private Transform checkpointSpawn;

    private void Start()
    {
        if (SimpleCheckpoint.checkpointReached && checkpointSpawn != null)
        {
            transform.position = checkpointSpawn.position;
            transform.rotation = checkpointSpawn.rotation;
        }
        else if (initialSpawn != null)
        {
            transform.position = initialSpawn.position;
            transform.rotation = initialSpawn.rotation;
        }
    }
}