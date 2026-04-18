using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ui : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI subtitleText = default;

    public static Ui Instance;

    public void Awake()
    {
        Instance = this;
        ClearSubtitle();
    }

    public void SetSubtitle(string subtitle,float delay)
    {
        subtitleText.text = subtitle;

        StartCoroutine(ClearAfterSeconds(delay));
    }

    public void ClearSubtitle()
    {
        subtitleText.text = "";
    }

    private IEnumerator ClearAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearSubtitle();
    }
}
