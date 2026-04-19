using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("Default Spawn")]
    [SerializeField] private Transform defaultSpawnPoint;

    private Vector3 respawnPosition;
    private Quaternion respawnRotation;
    private bool hasInitializedSpawn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (defaultSpawnPoint != null)
        {
            respawnPosition = defaultSpawnPoint.position;
            respawnRotation = defaultSpawnPoint.rotation;
            hasInitializedSpawn = true;
        }
    }

    public void SetDefaultSpawn(Transform spawnPoint)
    {
        if (spawnPoint == null)
            return;

        if (hasInitializedSpawn)
            return;

        respawnPosition = spawnPoint.position;
        respawnRotation = spawnPoint.rotation;
        hasInitializedSpawn = true;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null)
            return;

        respawnPosition = checkpoint.position;
        respawnRotation = checkpoint.rotation;
        hasInitializedSpawn = true;
    }

    public Vector3 GetRespawnPosition()
    {
        return respawnPosition;
    }

    public Quaternion GetRespawnRotation()
    {
        return respawnRotation;
    }

    public bool HasSpawnPoint()
    {
        return hasInitializedSpawn;
    }
}