using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    private static readonly Color ThemeColor = new Color(0.18f, 0.09f, 0.08f, 1f);
    private const float FadeDuration = 0.35f;

    private static SceneFader instance;

    private CanvasGroup canvasGroup;

    private static SceneFader Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SceneFader");
                instance = go.AddComponent<SceneFader>();
                instance.Build();
                DontDestroyOnLoad(go);
            }

            return instance;
        }
    }

    private void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        gameObject.AddComponent<CanvasScaler>();

        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(transform, false);

        Image image = imageGO.AddComponent<Image>();
        image.color = ThemeColor;
        image.raycastTarget = true;

        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public static void LoadScene(string sceneName)
    {
        Instance.StartCoroutine(Instance.FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        canvasGroup.blocksRaycasts = true;

        yield return Fade(0f, 1f);

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (load != null && !load.isDone)
        {
            yield return null;
        }

        yield return Fade(1f, 0f);

        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        canvasGroup.alpha = from;

        while (t < FadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / FadeDuration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
