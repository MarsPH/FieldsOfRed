using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FootstepSounds : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] walkGrassSounds;  // 5 sounds
    public AudioClip[] runGrassSounds;   // 5 sounds

    public float walkStepRate = 0.5f;
    public float runStepRate = 0.3f;

    public float pitchVariation = 0.1f;

    private CharacterController controller;
    private float stepTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        bool isMoving = controller.velocity.magnitude > 0.1f;

        if (!isMoving || !controller.isGrounded)
        {
            stepTimer = 0f;
            return;
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float stepRate = isRunning ? runStepRate : walkStepRate;

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep(isRunning);
            stepTimer = stepRate;
        }
    }

    void PlayFootstep(bool isRunning)
    {
        AudioClip[] clips = isRunning ? runGrassSounds : walkGrassSounds;

        if (clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clip);
    }
}