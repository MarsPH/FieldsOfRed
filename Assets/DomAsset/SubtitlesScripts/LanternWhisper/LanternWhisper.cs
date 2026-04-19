using UnityEngine;

public class LanternWhispers : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform monster;
    public PlayerHiding playerHiding;
    public LanternToggle lanternToggle;

    [Header("Distances")]
    public float dangerDistance = 10f;

    [Header("Voice Lines")]
    public AudioObject[] dangerLines;
    public AudioObject[] bushLines;

    [Header("Timing")]
    public float minDelay = 4f;
    public float maxDelay = 8f;

    [Range(0f, 1f)]
    public float speakChance = 0.7f;

    [Header("Debug")]
    public bool drawDebug = true;
    public Color dangerRangeColor = new Color(1f, 0f, 0f, 0.2f);
    public Color hiddenColor = new Color(0f, 1f, 0f, 0.2f);

    private float nextSpeakTime;

    void Update()
    {
        if (player == null || monster == null || playerHiding == null || lanternToggle == null)
            return;

        if (Time.time < nextSpeakTime)
            return;

        float distance = Vector3.Distance(player.position, monster.position);

        bool isHidden = playerHiding.IsHidden;
        bool isLanternOn = lanternToggle.IsOn();
        bool inDangerRange = distance <= dangerDistance;

        if (inDangerRange && isLanternOn && dangerLines.Length > 0)
        {
            TrySpeak(dangerLines);
        }
        else if (isHidden && bushLines.Length > 0)
        {
            TrySpeak(bushLines);
        }
    }

    void TrySpeak(AudioObject[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            SetNextTime();
            return;
        }

        if (Random.value > speakChance)
        {
            SetNextTime();
            return;
        }

        if (Vocals.instance == null)
        {
            SetNextTime();
            return;
        }

        int index = Random.Range(0, lines.Length);
        Vocals.instance.Say(lines[index]);

        SetNextTime();
    }

    void SetNextTime()
    {
        nextSpeakTime = Time.time + Random.Range(minDelay, maxDelay);
    }

    void OnDrawGizmos()
    {
        if (!drawDebug || monster == null)
            return;

        Gizmos.color = dangerRangeColor;
        Gizmos.DrawSphere(monster.position, dangerDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(monster.position, dangerDistance);

        if (player != null)
        {
            float distance = Vector3.Distance(player.position, monster.position);
            bool inDangerRange = distance <= dangerDistance;

            Gizmos.color = inDangerRange ? Color.red : Color.yellow;
            Gizmos.DrawLine(monster.position, player.position);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(player.position, 0.2f);
        }

        if (playerHiding != null && player != null)
        {
            Gizmos.color = playerHiding.IsHidden ? hiddenColor : new Color(0.3f, 0.3f, 0.3f, 0.15f);
            Gizmos.DrawCube(player.position + Vector3.up * 1.2f, new Vector3(0.6f, 0.6f, 0.6f));
        }
    }
}