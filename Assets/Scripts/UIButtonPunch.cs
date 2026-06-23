using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonPunch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float pressedScale = 0.92f;
    public float duration = 0.08f;

    private Vector3 originalScale;
    private Coroutine activeRoutine;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Play("button_click");
        StartPunch(originalScale * pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartPunch(originalScale);
    }

    private void StartPunch(Vector3 target)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        if (gameObject.activeInHierarchy)
        {
            activeRoutine = StartCoroutine(ScaleTo(target));
        }
    }

    private IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        transform.localScale = target;
    }
}
