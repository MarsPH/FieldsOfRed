using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSource;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 1.5f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🎵 Play a looping ambient sound
    public void PlayAmbient(AudioClip clip, float volume = 1f)
    {
        if (ambientSource.clip == clip)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeToNewClip(clip, volume));
    }

    // 🌫️ Fade to new sound
    private IEnumerator FadeToNewClip(AudioClip newClip, float targetVolume)
    {
        // Fade out
        while (ambientSource.volume > 0.01f)
        {
            ambientSource.volume -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        ambientSource.Stop();
        ambientSource.clip = newClip;
        ambientSource.loop = true;
        ambientSource.Play();

        // Fade in
        while (ambientSource.volume < targetVolume)
        {
            ambientSource.volume += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        ambientSource.volume = targetVolume;
    }

   
    public void StopAmbient()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        ambientSource.Stop();
    }

    // 🔊 Set volume manually
    public void SetVolume(float volume)
    {
        ambientSource.volume = volume;
    }
}