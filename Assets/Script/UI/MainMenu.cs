using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Persistent";
    [SerializeField] GameObject settingsPanel;
    [SerializeField] TMP_Text startButtonLabel;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (startButtonLabel != null)
            startButtonLabel.text = SaveSystem.HasSave() ? "Continue" : "Start";
    }

    public void OnStartClicked()
    {
        // Only request story intro for a fresh start flow.
        GameStartContext.SetStoryIntroForNextLoad(!SaveSystem.HasSave());
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettingsClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
