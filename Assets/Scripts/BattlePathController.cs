using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattlePathController : MonoBehaviour
{
    public GameObject nodePrefab;
    public RectTransform content;
    public RectTransform viewport;
    public PathScrollDrag scrollDrag;
    public Text statusText;
    public Text powerText;

    public int nodeCount = 15;
    public float nodeSpacingX = 220f;
    public float zigzagAmplitude = 100f;
    public float edgePadding = 160f;

    private int[] maxHP;
    private int[] currentHP;
    private BattlePathNodeView[] views;
    private int currentNodeIndex;
    private bool isEroding;

    void Start()
    {
        BuildNodes();
        LoadState();
        RefreshAll();

        int pending = PlayerPathProgress.TakePendingPower();
        if (pending > 0)
        {
            StartCoroutine(ErodeRoutine(pending));
        }
        else
        {
            if (powerText)
            {
                powerText.text = "";
            }

            FocusCurrentNode();
        }
    }

    private void BuildNodes()
    {
        maxHP = new int[nodeCount];
        currentHP = new int[nodeCount];
        views = new BattlePathNodeView[nodeCount];

        float contentWidth = edgePadding * 2f + nodeSpacingX * (nodeCount - 1);
        content.sizeDelta = new Vector2(contentWidth, content.sizeDelta.y);

        // Nodes anchor to content's bottom-left corner, so without this offset the whole
        // zigzag path renders well below content's (and the section dividers') center line.
        float centerOffset = content.sizeDelta.y * 0.5f;

        for (int i = 0; i < nodeCount; i++)
        {
            maxHP[i] = 200 + i * 50;

            bool isMilestone = (i + 1) % 5 == 0;
            if (isMilestone)
            {
                // Milestones are tougher mini-checkpoints, giving the player a concrete mid-term goal.
                maxHP[i] = Mathf.RoundToInt(maxHP[i] * 1.5f);
            }

            GameObject instance = Instantiate(nodePrefab, content);
            RectTransform rt = instance.GetComponent<RectTransform>();
            float x = edgePadding + i * nodeSpacingX;
            float y = centerOffset + ((i % 2 == 0) ? -zigzagAmplitude : zigzagAmplitude);
            rt.anchoredPosition = new Vector2(x, y);

            views[i] = instance.GetComponent<BattlePathNodeView>();

            if (isMilestone && i < nodeCount - 1)
            {
                CreateSectionDivider(x + nodeSpacingX * 0.5f);
            }
        }
    }

    private void CreateSectionDivider(float x)
    {
        GameObject dividerGO = new GameObject("SectionDivider");
        dividerGO.transform.SetParent(content, false);

        Image dividerImage = dividerGO.AddComponent<Image>();
        dividerImage.color = new Color(0.95f, 0.78f, 0.2f, 0.55f);

        RectTransform rt = dividerImage.rectTransform;
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, 0f);
        rt.sizeDelta = new Vector2(8f, 500f);
    }

    private void LoadState()
    {
        currentNodeIndex = Mathf.Clamp(PlayerPathProgress.NodeIndex, 0, nodeCount - 1);

        for (int i = 0; i < nodeCount; i++)
        {
            if (i < currentNodeIndex)
            {
                currentHP[i] = 0;
            }
            else if (i == currentNodeIndex)
            {
                int saved = PlayerPathProgress.CurrentNodeHP;
                currentHP[i] = saved >= 0 ? saved : maxHP[i];
            }
            else
            {
                currentHP[i] = maxHP[i];
            }
        }
    }

    private void RefreshAll()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            bool defeated = i < currentNodeIndex || (i == currentNodeIndex && currentHP[i] <= 0);
            bool isCurrent = i == currentNodeIndex && !defeated;
            views[i].SetState(currentHP[i], maxHP[i], isCurrent, defeated);
        }

        if (statusText)
        {
            statusText.text = "Düğüm " + (currentNodeIndex + 1) + " / " + nodeCount;
        }
    }

    public void OnWatchAdForPowerClicked()
    {
        if (isEroding)
        {
            return;
        }

        AdsManager.ShowRewardedAd(
            onRewardEarned: () => StartCoroutine(ErodeRoutine(300)),
            onClosedWithoutReward: null);
    }

    private IEnumerator ErodeRoutine(int power)
    {
        isEroding = true;

        if (powerText)
        {
            powerText.text = "Kalan Güç: " + power;
        }

        while (power > 0)
        {
            int damage = Mathf.Min(power, currentHP[currentNodeIndex]);
            currentHP[currentNodeIndex] -= damage;
            power -= damage;
            AudioManager.Play("battle_hit");

            if (powerText)
            {
                powerText.text = "Kalan Güç: " + power;
            }

            RefreshAll();
            FocusCurrentNode();
            yield return new WaitForSeconds(0.4f);

            if (currentHP[currentNodeIndex] <= 0)
            {
                AudioManager.Play("battle_node_clear");

                if (currentNodeIndex < nodeCount - 1)
                {
                    currentNodeIndex++;
                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    break;
                }
            }
        }

        PlayerPathProgress.NodeIndex = currentNodeIndex;
        PlayerPathProgress.CurrentNodeHP = currentHP[currentNodeIndex];

        if (powerText)
        {
            powerText.text = "Güç Tükendi";
        }

        RefreshAll();
        FocusCurrentNode();

        isEroding = false;
    }

    private void FocusCurrentNode()
    {
        float targetX = edgePadding + currentNodeIndex * nodeSpacingX;
        scrollDrag.SetContentX(viewport.rect.width * 0.5f - targetX);
    }

    public void OnContinueClicked()
    {
        SceneFader.LoadScene("LevelSelect");
    }
}
