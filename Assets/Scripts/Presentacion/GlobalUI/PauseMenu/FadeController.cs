using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float duration = 0.3f;

    public IEnumerator FadeIn()
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(t / duration);
            yield return null;
        }
        SetAlpha(1);
    }

    public IEnumerator FadeOut()
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(1 - (t / duration));
            yield return null;
        }
        SetAlpha(0);
    }

    void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}