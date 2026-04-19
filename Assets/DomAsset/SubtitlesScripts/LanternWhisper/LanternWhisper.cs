using UnityEngine;

public class LanternWhispers : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform monster;

    [Header("Distances")]
    public float dangerDistance = 10f;

    [Header("State")]
    public bool isInBush = false;

    [Header("Voice Lines")]
    public AudioObject[] dangerLines;
    public AudioObject[] bushLines;

    [Header("Timing")]
    public float minDelay = 4f;
    public float maxDelay = 8f;

    [Range(0f, 1f)]
    public float speakChance = 0.7f;

    float nextSpeakTime;

    void Update()
    {
        if (Time.time < nextSpeakTime)
            return;

        float distance = Vector3.Distance(player.position, monster.position);

        // Decide what type of line to play
        if (distance <= dangerDistance && dangerLines.Length > 0)
        {
            TrySpeak(dangerLines);
        }
        else if (isInBush && bushLines.Length > 0)
        {
            TrySpeak(bushLines);
        }
    }

    void TrySpeak(AudioObject[] lines)
    {
        // Chance check (prevents robotic spam)
        if (Random.value > speakChance)
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
}