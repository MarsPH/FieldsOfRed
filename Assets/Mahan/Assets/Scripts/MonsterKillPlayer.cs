using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class MonsterKillPlayer : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string endSceneName = "EndScene";

    [Header("Target")]
    [SerializeField] private string playerTag = "Player";

    [Header("Death Audio")]
    [SerializeField] private AudioSource deathAudioSource;
    [SerializeField] private AudioClip deathSound;

    [Header("Fade")]
    [SerializeField] private Image redFadeImage;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] [Min(0.05f)] private float fadeDurationMultiplier = 1f;
    [SerializeField] private bool holdUntilSoundEnds = true;

    [Header("Options")]
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private bool disablePlayerOnDeath = true;

    private bool isLoading;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (useTrigger)
            col.isTrigger = true;

        if (deathAudioSource == null)
            deathAudioSource = GetComponent<AudioSource>();

        if (redFadeImage != null)
        {
            Color c = redFadeImage.color;
            c.a = 0f;
            redFadeImage.color = c;
            redFadeImage.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger)
            return;

        TryKillPlayer(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useTrigger)
            return;

        TryKillPlayer(collision.gameObject);
    }

    private void TryKillPlayer(GameObject other)
    {
        if (isLoading)
            return;

        if (!other.CompareTag(playerTag))
            return;

        isLoading = true;

        if (disablePlayerOnDeath)
            DisablePlayer(other);

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        float soundDuration = 0f;

        if (deathAudioSource != null && deathSound != null)
        {
            deathAudioSource.Stop();
            deathAudioSource.clip = deathSound;
            deathAudioSource.loop = false;
            deathAudioSource.Play();
            soundDuration = deathSound.length;
        }

        float fadeDuration = soundDuration;

        if (soundDuration > 0f)
            fadeDuration = soundDuration * fadeDurationMultiplier;

        if (fadeDuration <= 0f)
            fadeDuration = 0.01f;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            float t = Mathf.Clamp01(timer / fadeDuration);

            if (redFadeImage != null)
            {
                Color c = redFadeImage.color;
                c.a = t;
                redFadeImage.color = c;
            }

            yield return null;
        }

        if (redFadeImage != null)
        {
            Color finalColor = redFadeImage.color;
            finalColor.a = 1f;
            redFadeImage.color = finalColor;
        }

        if (holdUntilSoundEnds && soundDuration > fadeDuration)
        {
            float remaining = soundDuration - fadeDuration;

            if (remaining > 0f)
            {
                if (useUnscaledTime)
                    yield return new WaitForSecondsRealtime(remaining);
                else
                    yield return new WaitForSeconds(remaining);
            }
        }

        LoadEndScene();
    }

    private void DisablePlayer(GameObject player)
    {
        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();

        for (int i = 0; i < scripts.Length; i++)
        {
            scripts[i].enabled = false;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void LoadEndScene()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(endSceneName);
    }
}