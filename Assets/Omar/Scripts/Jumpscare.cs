using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Jumpscare : MonoBehaviour
{
    [SerializeField] private Image jumpscareImage;
    [SerializeField] private AudioSource jumpscareSound;
    [SerializeField] private float displayTime = 2f; // how long it stays on screen

    private void Start()
    {
        jumpscareImage.gameObject.SetActive(false); // hidden at first
        StartCoroutine(PlayJumpscare());
    }

    private IEnumerator PlayJumpscare()
    {
        yield return new WaitForSeconds(0.1f); // tiny delay so scene fully loads

        jumpscareImage.gameObject.SetActive(true);
        jumpscareSound.Play();

        yield return new WaitForSeconds(displayTime);

        // After jumpscare, do whatever — fade out, show menu, etc.
        jumpscareImage.gameObject.SetActive(false);
    }
}