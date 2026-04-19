using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private void Start()
    {
        if (RespawnManager.Instance == null)
            return;

        if (!RespawnManager.Instance.HasSpawnPoint())
            return;

        transform.position = RespawnManager.Instance.GetRespawnPosition();
        transform.rotation = RespawnManager.Instance.GetRespawnRotation();
    }
}