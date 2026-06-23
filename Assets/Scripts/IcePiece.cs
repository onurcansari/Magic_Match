using UnityEngine;

public class IcePiece : MonoBehaviour
{
    private static readonly Color FrozenCellColor = new Color(1f, 1f, 1f, 0.8f);

    private GameObject overlayGO;

    public bool IsFrozen { get; private set; } = true;

    void Awake()
    {
        overlayGO = new GameObject("IceCellOverlay");
        overlayGO.transform.SetParent(transform, false);
        // The piece root sits at the cell's top-left corner (the same offset the
        // piece's own BoxCollider2D uses), so shift the overlay down-right to land
        // centered on the actual cell instead of on the corner between 4 cells.
        overlayGO.transform.localPosition = new Vector3(0.5f, -0.5f, 0f);

        Texture2D tex = Texture2D.whiteTexture;
        Sprite whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);

        SpriteRenderer overlaySprite = overlayGO.AddComponent<SpriteRenderer>();
        overlaySprite.sprite = whiteSprite;
        overlaySprite.color = FrozenCellColor;
        overlaySprite.sortingOrder = 0;
    }

    public void Crack()
    {
        IsFrozen = false;

        if (overlayGO != null)
        {
            Destroy(overlayGO);
        }
    }
}
