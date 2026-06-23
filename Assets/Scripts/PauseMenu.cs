using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject panel;
    public Grid grid;

    private bool isPaused;

    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (grid == null)
        {
            grid = FindObjectOfType<Grid>();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;

        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (grid != null)
        {
            grid.isPaused = true;
        }

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (grid != null)
        {
            grid.isPaused = false;
        }

        Time.timeScale = 1f;
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneFader.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneFader.LoadScene("MainScene");
    }
}
