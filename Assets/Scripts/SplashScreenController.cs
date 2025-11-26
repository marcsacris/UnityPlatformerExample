using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScreenController : MonoBehaviour
{
    [Header("UI")]
    public Image logoImage;           // Assign your logo UI Image in the Inspector
    public CanvasGroup logoCanvasGroup; // optional: use CanvasGroup if you want to fade the whole group

    [Header("Timings (seconds)")]
    public float fadeInDuration = 0.7f;
    public float displayDuration = 1.5f;
    public float fadeOutDuration = 0.7f;

    [Header("Next Scene")]
    public string nextSceneName = "MainMenu";

    [Header("Optional")]
    public bool useCanvasGroup = false; // If true, fades CanvasGroup.alpha; else fades Image.color.a
    public bool scaleOnShow = true;
    public Vector3 startScale = new Vector3(0.9f, 0.9f, 0.9f);
    public Vector3 endScale = Vector3.one;

    void Start()
    {
        // sanity checks
        if (!useCanvasGroup && logoImage == null)
        {
            Debug.LogError("SplashScreenController: logoImage is not assigned.");
            return;
        }

        if (useCanvasGroup && logoCanvasGroup == null)
        {
            Debug.LogError("SplashScreenController: logoCanvasGroup is not assigned while useCanvasGroup=true.");
            return;
        }

        // initialize alpha & scale
        if (useCanvasGroup)
            logoCanvasGroup.alpha = 0f;
        else
        {
            Color c = logoImage.color;
            c.a = 0f;
            logoImage.color = c;
        }

        if (scaleOnShow)
        {
            if (logoImage != null) logoImage.rectTransform.localScale = startScale;
            if (logoCanvasGroup != null && logoCanvasGroup.GetComponent<RectTransform>() != null)
                logoCanvasGroup.GetComponent<RectTransform>().localScale = startScale;
        }

        StartCoroutine(SplashSequence());
    }

    IEnumerator SplashSequence()
    {
        // Fade in
        yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));

        // optional scale animate during display
        if (scaleOnShow)
        {
            float t = 0f;
            while (t < displayDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / displayDuration);
                Vector3 s = Vector3.Lerp(startScale, endScale, p);
                if (logoImage != null) logoImage.rectTransform.localScale = s;
                if (logoCanvasGroup != null && logoCanvasGroup.GetComponent<RectTransform>() != null)
                    logoCanvasGroup.GetComponent<RectTransform>().localScale = s;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(displayDuration);
        }

        // Fade out
        yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));

        // Load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("SplashScreenController: nextSceneName is empty.");
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = (duration > 0f) ? timer / duration : 1f;
            float alpha = Mathf.Lerp(from, to, t);

            if (useCanvasGroup && logoCanvasGroup != null)
                logoCanvasGroup.alpha = alpha;
            else if (logoImage != null)
            {
                Color c = logoImage.color;
                c.a = alpha;
                logoImage.color = c;
            }

            yield return null;
        }

        // ensure final alpha
        if (useCanvasGroup && logoCanvasGroup != null) logoCanvasGroup.alpha = to;
        else if (logoImage != null)
        {
            Color c = logoImage.color;
            c.a = to;
            logoImage.color = c;
        }
    }
}
