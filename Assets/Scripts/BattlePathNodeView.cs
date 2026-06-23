using UnityEngine;
using UnityEngine.UI;

public class BattlePathNodeView : MonoBehaviour
{
    public Image icon;
    public Image fillImage;
    public Text hpLabel;
    public Text statusLabel;

    public void SetState(int currentHP, int maxHP, bool isCurrent, bool isDefeated)
    {
        float fill = maxHP > 0 ? Mathf.Clamp01((float)currentHP / maxHP) : 0f;
        fillImage.fillAmount = fill;
        hpLabel.text = "Güç: " + currentHP + " / " + maxHP;

        if (isDefeated)
        {
            icon.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            statusLabel.text = "Geçildi";
        }
        else if (isCurrent)
        {
            icon.color = new Color(0.92f, 0.75f, 0.35f, 1f);
            statusLabel.text = "Buradasın";
        }
        else
        {
            icon.color = new Color(0.75f, 0.2f, 0.18f, 1f);
            statusLabel.text = "Bekliyor";
        }
    }
}
