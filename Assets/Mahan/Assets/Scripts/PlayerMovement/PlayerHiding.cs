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

    [Header("Screen Effects")]
    [SerializeField] private GameObject hideOverlay;
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
    }

    private void Start()
    {
        if (cameraHolder != null)
        {
            cameraStartLocalPosition = cameraHolder.localPosition;
        }

        if (hideOverlay != null)
        {
            hideOverlay.SetActive(false);
        }

        if (vignetteGroup != null)
        {
            vignetteGroup.alpha = 0f;
        }

        SetExtraHiddenObjects(false);
        ApplyNormalMovementState();
    }

    private void Update()
    {
        UpdateCameraPosition();
        UpdateVignette();
    }

    public void EnterHideZone(HideZone zone)
    {
        if (zone == null)
            return;

        currentZone = zone;
        canHide = true;

        if (!isHidden && !IsTransitioning)
        {
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

        if (IsTransitioning)
        {
            CancelEnterRoutine();
            currentState = HideState.None;
            ApplyNormalMovementState();
        }

        if (isHidden)
        {
            ForceStopHidingImmediate();
        }
    }

    private IEnumerator EnterHideRoutine()
    {
        currentState = HideState.Entering;
        ApplyEnteringMovementState();

        if (enterHideDuration > 0f)
        {
            yield return new WaitForSeconds(enterHideDuration);
        }

        enterRoutine = null;

        if (!canHide || currentZone == null)
        {
            currentState = HideState.None;
            ApplyNormalMovementState();
            yield break;
        }

        isHidden = true;
        currentState = HideState.Hidden;

        ApplyHiddenMovementState();
        SetHiddenVisuals(true);
    }

    private void ForceStopHidingImmediate()
    {
        CancelEnterRoutine();

        isHidden = false;
        currentState = HideState.None;

        SetHiddenVisuals(false);
        ApplyNormalMovementState();
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
        if (hideOverlay != null)
        {
            hideOverlay.SetActive(hidden);
        }

        SetExtraHiddenObjects(hidden);

        currentTargetCameraOffset = hidden ? hiddenCameraYOffset : 0f;
        currentTargetVignetteAlpha = hidden ? hiddenVignetteAlpha : 0f;
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
}