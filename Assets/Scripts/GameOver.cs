using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{

    public GameObject screenParent;
    public GameObject scoreParent;
    public UnityEngine.UI.Text loseText;
    public UnityEngine.UI.Text scoreText;
    public GameObject watchAdButton;
    public UnityEngine.UI.Image[] stars;



    // Start is called before the first frame update
    void Start()
    {
        screenParent.SetActive(false);
        for(int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = false;
        }

        if (watchAdButton)
        {
            watchAdButton.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowLose(int score)
    {
        screenParent.SetActive(true);
        scoreParent.SetActive(false);

        if (watchAdButton)
        {
            watchAdButton.SetActive(true);
        }

        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            animator.Play("GameOverShow");
        }

        ApplyScorePower(score);
    }

    public void ShowWin(int score, int starCount)
    {
        screenParent.SetActive(true);
        loseText.enabled = false;

        if (watchAdButton)
        {
            watchAdButton.SetActive(false);
        }

        scoreText.text = score.ToString();
        scoreText.enabled = false;


        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            animator.Play("GameOverShow");
        }

        StartCoroutine(ShowWinCoroutine(starCount));

        ApplyScorePower(score);
    }

    private void ApplyScorePower(int score)
    {
        string key = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_BestScore";
        int bestScore = PlayerPrefs.GetInt(key, 0);

        if (score > bestScore)
        {
            int delta = score - bestScore;
            PlayerPrefs.SetInt(key, score);
            PlayerPathProgress.AddPendingPower(delta / 20);
        }
    }

    private IEnumerator ShowWinCoroutine(int starCount)
    {
        yield return new WaitForSeconds(0.5f);

        if (starCount < stars.Length)
        {
            for (int i = 0; i <= starCount; i++)
            {
                stars[i].enabled = true;

                if (i > 0)
                {
                    stars[i - 1].enabled = false;
                }

                yield return new WaitForSeconds(0.5f);

            }
        }

        scoreText.enabled = true;
    }

    public void OnWatchAdForMovesClicked()
    {
        Level level = FindObjectOfType<Level>();

        if (level == null)
        {
            return;
        }

        AdsManager.ShowRewardedAd(
            onRewardEarned: () =>
            {
                level.AddBonusMoves(5);
                level.ResumeAfterBonus();
                screenParent.SetActive(false);
            },
            onClosedWithoutReward: null);
    }

    public void OnReplayClicked()
    {
        SceneFader.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void OnDoneClicked()
    {
        SceneFader.LoadScene("BattlePath");
    }
}
