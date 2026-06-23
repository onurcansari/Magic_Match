using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public RectTransform leftBannerPlaceholder;
    public RectTransform rightBannerPlaceholder;

    void Start()
    {
        if (leftBannerPlaceholder)
        {
            AdsManager.ShowBanner("MainMenuLeft", leftBannerPlaceholder);
        }

        if (rightBannerPlaceholder)
        {
            AdsManager.ShowBanner("MainMenuRight", rightBannerPlaceholder);
        }
    }

    public void OnPlayClicked()
    {
        SceneFader.LoadScene("LevelSelect");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public void OnBattlePathClicked()
    {
        SceneFader.LoadScene("BattlePath");
    }
}
