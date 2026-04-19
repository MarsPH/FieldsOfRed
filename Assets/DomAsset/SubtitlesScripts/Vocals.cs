using UnityEngine;

public class Vocals : MonoBehaviour
{
  private AudioSource audioSource;
  
  public static Vocals instance;

  private void Awake()
  {
    if (instance != null && instance != this)
    {
      Destroy(gameObject);
      return;
    }

    instance = this;

    audioSource = gameObject.AddComponent<AudioSource>();
  }


  public void Say(AudioObject clip)
  {
    if (audioSource.isPlaying)
      audioSource.Stop();
    audioSource.PlayOneShot(clip.audioClip);
    Ui.Instance.SetSubtitle(clip.subtitle, clip.audioClip.length);
  }
  
}
