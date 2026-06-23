using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ScreenBackgroundFit : MonoBehaviour
{
    public float overscan = 1.05f;

    void Start()
    {
        Camera cam = Camera.main;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (cam == null || !cam.orthographic || spriteRenderer.sprite == null)
        {
            return;
        }

        float viewportHeight = cam.orthographicSize * 2f;
        float viewportWidth = viewportHeight * cam.aspect;

        Vector2 nativeSize = spriteRenderer.sprite.bounds.size;
        float scale = Mathf.Max(viewportWidth / nativeSize.x, viewportHeight / nativeSize.y) * overscan;

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
