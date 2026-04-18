using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WASDSound;

namespace WASDSound
{
    public class WASDFootstepSource : MonoBehaviour
    {
        [Tooltip("If the Audiosource isn't set, a new one gets created in Awake")]
        public AudioSource audioSource;
        [Tooltip("Volume value, set on Start"), SerializeField]
        float volume = 0.8f;

        [Tooltip("AudioSource gets set to 3D"), SerializeField]
        bool sound3D = true;
        [Tooltip("When true, Pitch gets randomised, when a sound gets played"), SerializeField] 
        bool randomisePitch = false;
        [Tooltip("Strength of randomisation"), SerializeField]
        float randomPitchRange = 1;
        [Tooltip("General Offset to make steps higher or lower"), SerializeField]
        float pitchOffset = 1;

        [Tooltip("You find the file in WASDSound/Assets"), SerializeField]
        WASDFootstepManager footsteps;

        [Tooltip("Default Action that gets played, when PlayFootstep() gets called"), SerializeField]
        WASDEnumAction action = WASDEnumAction.Walk;
        [Tooltip("Default Material that gets played, when PlayFootstep() gets called"), SerializeField]
        WASDEnumMaterial material = WASDEnumMaterial.Stone;

        private void Awake()
        {
            if (this.gameObject.GetComponent<AudioSource>())
            {
                audioSource = this.gameObject.GetComponent<AudioSource>();
            }
            else
            {
                audioSource = this.gameObject.AddComponent<AudioSource>();
            }

            if (sound3D)
            {
                audioSource.spatialBlend = 1;
            }
            else
            {
                audioSource.spatialBlend = 0;
            }
        }

        private void Start()
        {
            audioSource.volume = volume;
        }

        public void PlayFootstep()
        {
            AudioClip clip = footsteps.GetAudioClip(action, material);

            float rPitch = pitchOffset;
            if (randomisePitch) rPitch = Random.Range(pitchOffset - randomPitchRange, pitchOffset + randomPitchRange);

            audioSource.pitch = rPitch;
            audioSource.PlayOneShot(clip);
        }

        public void PlayFootstepByAction(WASDEnumAction a)
        {
            action = a;
            PlayFootstep();
        }

        public void PlayFootstepByAction(string actionName)
        {
            switch (actionName)
            {
                case "Drop":
                    action = WASDEnumAction.Drop;
                    break;
                case "Jump":
                    action = WASDEnumAction.Jump;
                    break;
                case "Run":
                    action = WASDEnumAction.Run;
                    break;
                case "Shuffle":
                    action = WASDEnumAction.Shuffle;
                    break;
                case "Sneak":
                    action = WASDEnumAction.Sneak;
                    break;
                case "Walk":
                    action = WASDEnumAction.Walk;
                    break;
                default:
                    action = WASDEnumAction.Walk;
                    break;
            }
            PlayFootstep();
        }

        public void PlayFootstepByAction(WASDEnumAction a, WASDEnumMaterial m)
        {
            action = a;
            material = m;
            PlayFootstep();
        }

        public void SetMaterial(WASDEnumMaterial m)
        {
            material = m;
        }

        public void SetAction(WASDEnumAction a)
        {
            action = a;
        }
    }

}
