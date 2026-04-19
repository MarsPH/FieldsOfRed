using UnityEngine;
using System.Collections;

public class LanternToggle : MonoBehaviour
{
    [Header("References")]
    public GameObject flameObject;
    public Light flameLight;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip turnOnSound;
    public AudioClip turnOffSound;
    
    [Header("Input")]
    public KeyCode toggleKey = KeyCode.E;

    [Header("State")]
    public bool startsOn = true;
    public bool canToggle = true;

    [Header("Timing")]
    public float turnOnTime = 0.4f;
    public float turnOffTime = 0.25f;

    [Header("Fuel")]
    public bool useFuel = true;
    public float maxFuel = 100f;
    public float currentFuel = 100f;
    public float fuelCostPerSecond = 1f;
    public float fuelCostPerToggleOn = 5f;
    public float fuelCostPerToggleOff = 0f;

    [Header("Behavior")]
    public bool autoTurnOffWhenNoFuel = true;

    private bool isOn;
    private bool isBusy;
    private Coroutine toggleRoutine;
    private bool hasInitialized;

    void Start()
    {
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
        isOn = startsOn;

        if (useFuel && currentFuel <= 0f)
            isOn = false;

        hasInitialized = false;
        ApplyStateImmediate(); // no sound allowed yet
        hasInitialized = true;
    }
    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && canToggle && !isBusy)
        {
            TryToggle();
        }

        if (useFuel && isOn)
        {
            currentFuel -= fuelCostPerSecond * Time.deltaTime;
            currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);

            if (currentFuel <= 0f && autoTurnOffWhenNoFuel)
            {
                StartToggle(false);
            }
        }
    }

    public void TryToggle()
    {
        if (isOn)
        {
            if (useFuel && currentFuel < fuelCostPerToggleOff)
                return;

            StartToggle(false);
        }
        else
        {
            if (useFuel && currentFuel < fuelCostPerToggleOn)
                return;

            StartToggle(true);
        }
    }

    private void StartToggle(bool turnOn)
    {
        if (toggleRoutine != null)
            StopCoroutine(toggleRoutine);

        toggleRoutine = StartCoroutine(ToggleRoutine(turnOn));
    }

    private IEnumerator ToggleRoutine(bool turnOn)
    {
        isBusy = true;

        float waitTime = turnOn ? turnOnTime : turnOffTime;
        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        if (useFuel)
        {
            if (turnOn)
            {
                if (currentFuel < fuelCostPerToggleOn)
                {
                    isBusy = false;
                    yield break;
                }

                currentFuel -= fuelCostPerToggleOn;
            }
            else
            {
                if (currentFuel < fuelCostPerToggleOff)
                {
                    isBusy = false;
                    yield break;
                }

                currentFuel -= fuelCostPerToggleOff;
            }

            currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
        }

        isOn = turnOn;
        ApplyStateImmediate();
        isBusy = false;
    }

    private void ApplyStateImmediate()
    {
        if (flameObject != null)
            flameObject.SetActive(isOn);

        if (flameLight != null)
            flameLight.enabled = isOn;

        if (!hasInitialized || audioSource == null)
            return;

        if (isOn && turnOnSound != null)
        {
            audioSource.PlayOneShot(turnOnSound);
        }
        else if (!isOn && turnOffSound != null)
        {
            audioSource.PlayOneShot(turnOffSound);
        }
    }
    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0f, maxFuel);
    }

    public bool IsOn()
    {
        return isOn;
    }

    public float GetFuelPercent()
    {
        if (maxFuel <= 0f)
            return 0f;

        return currentFuel / maxFuel;
    }
}