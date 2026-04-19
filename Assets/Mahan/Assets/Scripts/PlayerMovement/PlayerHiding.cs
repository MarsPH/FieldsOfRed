using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerHiding : MonoBehaviour
{
    public enum HideState
    {
        None,
        Entering,
        Hidden
    }

    [Header("References")]
    [SerializeField] private LanternToggle lantern;

    [Header("Rules")]
    [SerializeField] private bool requireLanternOffToHide = true;

    [Header("Timing")]
    [SerializeField] private float enterHideDuration = 0.6f;

    [Header("Movement")]
    [SerializeField] private bool lockMovementWhileEntering = true;
    [SerializeField] private bool lockJumpWhileEntering = true;
    [SerializeField] private bool lockMovementWhileHidden = false;
    [SerializeField] private bool lockJumpWhileHidden = true;
    [SerializeField] [Min(0f)] private float hiddenSpeedMultiplier = 0.35f;

    [Header("Camera")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float hiddenCameraYOffset = -0.35f;
    [SerializeField] private float cameraMoveSpeed = 8f;

    [Header("Audio")]
    [SerializeField] private AudioSource hideAudioSource;
    [SerializeField] private AudioClip enterHideSound;
    [SerializeField] private AudioClip exitHideSound;
    [SerializeField] [Range(0f, 1f)] private float hideSoundVolume = 1f;

    [Header("Animator Option")]
    [SerializeField] private Animator leavesAnimator;
    [SerializeField] private bool useAnimatorBool = false;
    [SerializeField] private string animatorHiddenBool = "IsHidden";
    [SerializeField] private bool useAnimatorTriggers = true;
    [SerializeField] private string animatorEnterTrigger = "EnterHide";
    [SerializeField] private string animatorExitTrigger = "ExitHide";

    [Header("Legacy Animation Option")]
    [SerializeField] private Animation leavesAnimation;
    [SerializeField] private string enterAnimationName = "LeavesOn";
    [SerializeField] private string exitAnimationName = "ReverseLeaves";

    [Header("Vignette")]
    [SerializeField] private CanvasGroup vignetteGroup;
    [SerializeField] [Range(0f, 1f)] private float hiddenVignetteAlpha = 0.45f;
    [SerializeField] private float vignetteFadeSpeed = 6f;

    [Header("Optional Hidden Objects")]
    [SerializeField] private GameObject[] enableWhileHidden;
    [SerializeField] private GameObject[] disableWhileHidden;

    [Header("Debug")]
    [SerializeField] private bool canHide;
    [SerializeField] private bool isHidden;
    [SerializeField] private HideState currentState = HideState.None;
    [SerializeField] private bool debugLogs = true;

    private PlayerMovement playerMovement;
    private HideZone currentZone;
    private Coroutine enterRoutine;

    private Vector3 cameraStartLocalPosition;
    private float currentTargetCameraOffset;
    private float currentTargetVignetteAlpha;

    public bool CanHide => canHide;
    public bool IsHidden => isHidden;
    public bool IsTransitioning => currentState == HideState.Entering;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (hideAudioSource == null)
            hideAudioSource = GetComponent<AudioSource>();

        if (lantern == null)
            lantern = GetComponentInChildren<LanternToggle>();
    }

    private void Start()
    {
        if (cameraHolder != null)
            cameraStartLocalPosition = cameraHolder.localPosition;

        if (vignetteGroup != null)
            vignetteGroup.alpha = 0f;

        SetExtraHiddenObjects(false);
        ApplyNormalMovementState();

        currentTargetCameraOffset = 0f;
        currentTargetVignetteAlpha = 0f;

        SetInitialLeavesState();
    }

    private void Update()
    {
        UpdateHideStateByLantern();
        UpdateCameraPosition();
        UpdateVignette();
    }

    private void UpdateHideStateByLantern()
    {
        if (!canHide || currentZone == null)
            return;

        bool allowedToHide = CanCurrentlyHide();

        if (!allowedToHide)
        {
            if (IsTransitioning)
            {
                CancelEnterRoutine();
                currentState = HideState.None;
                SetHiddenVisuals(false);
                ApplyNormalMovementState();
                LogDebug("Hide cancelled because lantern state does not allow hiding.");
            }

            if (isHidden)
            {
                ForceStopHidingImmediate();
                LogDebug("Forced out of hiding because lantern turned on.");
            }

            return;
        }

        if (!isHidden && !IsTransitioning)
        {
            PlayEnterHideSoundImmediate();
            enterRoutine = StartCoroutine(EnterHideRoutine());
            LogDebug("Started hiding because lantern state allows it.");
        }
    }

    private bool CanCurrentlyHide()
    {
        if (!canHide || currentZone == null)
            return false;

        if (!requireLanternOffToHide)
            return true;

        if (lantern == null)
            return true;

        return !lantern.IsOn();
    }

    public void EnterHideZone(HideZone zone)
    {
        if (zone == null)
            return;

        currentZone = zone;
        canHide = true;

        LogDebug("Entered hide zone: " + zone.name);

        if (CanCurrentlyHide() && !isHidden && !IsTransitioning)
        {
            PlayEnterHideSoundImmediate();
            enterRoutine = StartCoroutine(EnterHideRoutine());
        }
    }

    public void ExitHideZone(HideZone zone)
    {
        if (zone == null)
            return;

        if (currentZone != zone)
            return;

        canHide = false;
        currentZone = null;

        LogDebug("Exited hide zone: " + zone.name);

        if (IsTransitioning)
        {
            CancelEnterRoutine();
            currentState = HideState.None;
            SetHiddenVisuals(false);
            PlayExitHideSoundImmediate();
            ApplyNormalMovementState();
        }

        if (isHidden)
            ForceStopHidingImmediate();
    }

    private IEnumerator EnterHideRoutine()
    {
        currentState = HideState.Entering;
        LogDebug("EnterHideRoutine started.");

        SetHiddenVisuals(true);
        ApplyEnteringMovementState();

        if (enterHideDuration > 0f)
            yield return new WaitForSeconds(enterHideDuration);

        enterRoutine = null;

        if (!CanCurrentlyHide())
        {
            LogDebug("EnterHideRoutine aborted.");
            currentState = HideState.None;
            SetHiddenVisuals(false);
            ApplyNormalMovementState();
            yield break;
        }

        isHidden = true;
        currentState = HideState.Hidden;

        LogDebug("Player is now hidden.");
        ApplyHiddenMovementState();
    }

    private void ForceStopHidingImmediate()
    {
        CancelEnterRoutine();

        isHidden = false;
        currentState = HideState.None;

        SetHiddenVisuals(false);
        PlayExitHideSoundImmediate();
        ApplyNormalMovementState();

        LogDebug("Stopped hiding immediately.");
    }

    private void CancelEnterRoutine()
    {
        if (enterRoutine != null)
        {
            StopCoroutine(enterRoutine);
            enterRoutine = null;
        }
    }

    private void ApplyEnteringMovementState()
    {
        playerMovement.ResetMovementModifiers();

        if (lockMovementWhileEntering)
            playerMovement.SetMovementLocked(true);

        if (lockJumpWhileEntering)
            playerMovement.SetJumpLocked(true);
    }

    private void ApplyHiddenMovementState()
    {
        playerMovement.ResetMovementModifiers();

        if (lockMovementWhileHidden)
            playerMovement.SetMovementLocked(true);
        else
            playerMovement.SetSpeedMultiplier(hiddenSpeedMultiplier);

        if (lockJumpWhileHidden)
            playerMovement.SetJumpLocked(true);
    }

    private void ApplyNormalMovementState()
    {
        playerMovement.ResetMovementModifiers();
    }

    private void SetHiddenVisuals(bool hidden)
    {
        PlayLeaves(hidden);
        SetExtraHiddenObjects(hidden);

        currentTargetCameraOffset = hidden ? hiddenCameraYOffset : 0f;
        currentTargetVignetteAlpha = hidden ? hiddenVignetteAlpha : 0f;
    }

    private void PlayEnterHideSoundImmediate()
    {
        PlaySpecificHideSound(enterHideSound, "ENTER");
    }

    private void PlayExitHideSoundImmediate()
    {
        PlaySpecificHideSound(exitHideSound, "EXIT");
    }

    private void PlaySpecificHideSound(AudioClip clipToPlay, string soundLabel)
    {
        LogDebug("Trying to play " + soundLabel + " hide sound.");

        if (hideAudioSource == null)
        {
            LogDebug(soundLabel + " sound failed. No AudioSource.");
            return;
        }

        if (clipToPlay == null)
        {
            LogDebug(soundLabel + " sound failed. Clip is null.");
            return;
        }

        hideAudioSource.PlayOneShot(clipToPlay, hideSoundVolume);
        LogDebug(soundLabel + " PlayOneShot called: " + clipToPlay.name);
    }

    private void PlayLeaves(bool hidden)
    {
        if (leavesAnimator != null)
        {
            if (useAnimatorTriggers)
            {
                if (hidden)
                {
                    leavesAnimator.ResetTrigger(animatorExitTrigger);
                    leavesAnimator.SetTrigger(animatorEnterTrigger);
                }
                else
                {
                    leavesAnimator.ResetTrigger(animatorEnterTrigger);
                    leavesAnimator.SetTrigger(animatorExitTrigger);
                }
            }

            if (useAnimatorBool)
                leavesAnimator.SetBool(animatorHiddenBool, hidden);

            return;
        }

        if (leavesAnimation != null)
        {
            string clipToPlay = hidden ? enterAnimationName : exitAnimationName;

            if (!string.IsNullOrEmpty(clipToPlay) && leavesAnimation.GetClip(clipToPlay) != null)
                leavesAnimation.Play(clipToPlay);
        }
    }

    private void SetInitialLeavesState()
    {
        if (leavesAnimator != null)
        {
            if (useAnimatorBool)
                leavesAnimator.SetBool(animatorHiddenBool, false);

            return;
        }

        if (leavesAnimation != null)
        {
            if (!string.IsNullOrEmpty(exitAnimationName) && leavesAnimation.GetClip(exitAnimationName) != null)
            {
                leavesAnimation.Play(exitAnimationName);
                leavesAnimation[exitAnimationName].speed = 0f;
                leavesAnimation[exitAnimationName].time = leavesAnimation[exitAnimationName].length;
                leavesAnimation.Sample();
                leavesAnimation.Stop();
            }
        }
    }

    private void SetExtraHiddenObjects(bool hidden)
    {
        if (enableWhileHidden != null)
        {
            for (int i = 0; i < enableWhileHidden.Length; i++)
            {
                if (enableWhileHidden[i] != null)
                    enableWhileHidden[i].SetActive(hidden);
            }
        }

        if (disableWhileHidden != null)
        {
            for (int i = 0; i < disableWhileHidden.Length; i++)
            {
                if (disableWhileHidden[i] != null)
                    disableWhileHidden[i].SetActive(!hidden);
            }
        }
    }

    private void UpdateCameraPosition()
    {
        if (cameraHolder == null)
            return;

        Vector3 targetPosition = cameraStartLocalPosition + new Vector3(0f, currentTargetCameraOffset, 0f);

        cameraHolder.localPosition = Vector3.Lerp(
            cameraHolder.localPosition,
            targetPosition,
            cameraMoveSpeed * Time.deltaTime
        );
    }

    private void UpdateVignette()
    {
        if (vignetteGroup == null)
            return;

        vignetteGroup.alpha = Mathf.Lerp(
            vignetteGroup.alpha,
            currentTargetVignetteAlpha,
            vignetteFadeSpeed * Time.deltaTime
        );
    }

    private void LogDebug(string message)
    {
        if (!debugLogs)
            return;

        Debug.Log("[PlayerHiding] " + message, this);
    }
}