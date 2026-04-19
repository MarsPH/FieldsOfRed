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
        float duration = 0f;

        if (deathAudioSource != null && deathSound != null)
        {
            deathAudioSource.Stop();
            deathAudioSource.clip = deathSound;
            deathAudioSource.loop = false;
            deathAudioSource.Play();
            duration = deathSound.length;
        }

        if (duration <= 0f)
        {
            if (redFadeImage != null)
            {
                Color instantColor = redFadeImage.color;
                instantColor.a = 1f;
                redFadeImage.color = instantColor;
            }

            LoadEndScene();
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);

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