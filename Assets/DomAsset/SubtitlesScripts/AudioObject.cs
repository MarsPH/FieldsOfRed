using UnityEngine;

[CreateAssetMenu(fileName = "SubtitlesScript", menuName = "SubtitlesScript")]
public class AudioObject : ScriptableObject
{
  public AudioClip audioClip;
  public string subtitle;
}
