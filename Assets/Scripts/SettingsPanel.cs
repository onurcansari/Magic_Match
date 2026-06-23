using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public GameObject panel;
    public Text muteButtonText;

    void OnEnable()
    {
        RefreshLabel();
    }

    public void Open()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        RefreshLabel();
    }

    public void Close()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void OnToggleSoundClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMuted();
        }

        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (muteButtonText == null)
        {
            return;
        }

        bool muted = AudioManager.Instance != null && AudioManager.Instance.IsMuted;
        muteButtonText.text = muted ? "Sound: Off" : "Sound: On";
    }
}
