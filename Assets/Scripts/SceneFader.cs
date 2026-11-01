using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float defaultFadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // fades the screen to black and loads the specified scene.
    public void FadeToBlack(string sceneName)
    {
        StartCoroutine(FadeAndSwitchScene(sceneName, defaultFadeDuration));
    }

    // overload allowing a custom duration if needed.
    public void FadeToBlack(string sceneName, float duration)
    {
        StartCoroutine(FadeAndSwitchScene(sceneName, duration));
    }

    private IEnumerator FadeAndSwitchScene(string sceneName, float duration)
    {
        // fade Out
        yield return StartCoroutine(FadeRoutine(0f, 1f, duration));

        // load new scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // fade In
        yield return StartCoroutine(FadeRoutine(1f, 0f, duration));
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, float duration)
    {
        float timer = 0f;
        canvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}