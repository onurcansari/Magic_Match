using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public GameObject levelNodePrefab;
    public RectTransform content;
    public RectTransform viewport;
    public PathScrollDrag scrollDrag;
    public Sprite[] digitSprites;

    public int levelCount = 20;
    public int columns = 10;
    public float columnSpacing = 300f;
    public float rowOffset = 150f;
    public float edgePadding = 300f;
    public float singleDigitX = 0f;
    public float doubleDigitOffsetX = 32f;

    void Start()
    {
        BuildButtons();
        FocusFirstIncompleteLevel();
    }

    private void BuildButtons()
    {
        float contentWidth = edgePadding * 2f + columnSpacing * (columns - 1);
        content.sizeDelta = new Vector2(contentWidth, content.sizeDelta.y);

        bool firstLockedFound = false;

        for (int i = 0; i < levelCount; i++)
        {
            int levelNumber = i + 1;
            string levelName = "Level" + levelNumber;
            int column = i / 2;
            int row = i % 2;
            float x = edgePadding + column * columnSpacing;
            float y = row == 0 ? rowOffset : -rowOffset;

            GameObject instance = Instantiate(levelNodePrefab, content);
            RectTransform rt = instance.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, y);

            int score = PlayerPrefs.GetInt(levelName, 0);
            for (int starIdx = 1; starIdx <= 3; starIdx++)
            {
                Transform star = instance.transform.Find("star" + starIdx);
                star.gameObject.SetActive(starIdx <= score);
            }

            SetLevelNumber(instance.transform, levelNumber);

            bool locked = i > 0 && PlayerPrefs.GetInt("Level" + i, 0) < 1;
            instance.GetComponent<Button>().interactable = !locked;

            Transform lockOverlay = instance.transform.Find("LockOverlay");
            lockOverlay.gameObject.SetActive(locked);

            bool showUnlockButton = locked && !firstLockedFound;
            Transform watchAdButton = lockOverlay.Find("WatchAdToUnlockButton");
            watchAdButton.gameObject.SetActive(showUnlockButton);

            if (locked)
            {
                firstLockedFound = true;
            }

            if (showUnlockButton)
            {
                int previousLevelKey = i;
                watchAdButton.GetComponent<Button>().onClick.AddListener(() => OnWatchAdForUnlockClicked(previousLevelKey));
            }

            instance.GetComponent<Button>().onClick.AddListener(() => OnButtonPress(levelName));
        }
    }

    public void OnWatchAdForUnlockClicked(int previousLevelKey)
    {
        AdsManager.ShowRewardedAd(
            onRewardEarned: () =>
            {
                AudioManager.Play("level_unlock");
                PlayerPrefs.SetInt("Level" + previousLevelKey, 1);
                RebuildButtons();
            },
            onClosedWithoutReward: null);
    }

    private void RebuildButtons()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        BuildButtons();
    }

    private void SetLevelNumber(Transform node, int levelNumber)
    {
        Image digit1 = node.Find("Digit1").GetComponent<Image>();
        Image digit2 = node.Find("Digit2").GetComponent<Image>();

        if (levelNumber < 10)
        {
            digit1.sprite = digitSprites[levelNumber];
            digit1.rectTransform.anchoredPosition = new Vector2(singleDigitX, digit1.rectTransform.anchoredPosition.y);
            digit2.gameObject.SetActive(false);
        }
        else
        {
            int tens = levelNumber / 10;
            int ones = levelNumber % 10;

            digit1.sprite = digitSprites[tens];
            digit1.rectTransform.anchoredPosition = new Vector2(-doubleDigitOffsetX, digit1.rectTransform.anchoredPosition.y);

            digit2.sprite = digitSprites[ones];
            digit2.rectTransform.anchoredPosition = new Vector2(doubleDigitOffsetX, digit2.rectTransform.anchoredPosition.y);
            digit2.gameObject.SetActive(true);
        }
    }

    private void FocusFirstIncompleteLevel()
    {
        int targetIndex = levelCount - 1;
        for (int i = 0; i < levelCount; i++)
        {
            if (PlayerPrefs.GetInt("Level" + (i + 1), 0) == 0)
            {
                targetIndex = i;
                break;
            }
        }

        float targetX = edgePadding + (targetIndex / 2) * columnSpacing;
        scrollDrag.SetContentX(viewport.rect.width * 0.5f - targetX);
    }

    public void OnButtonPress(string levelName)
    {
        SceneFader.LoadScene(levelName);
    }

    public void OnBackClicked()
    {
        SceneFader.LoadScene("MainScene");
    }
}
