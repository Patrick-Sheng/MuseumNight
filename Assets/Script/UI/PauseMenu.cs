using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }
    public static bool IsPaused { get; private set; }

    [Header("Panels")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject settingsPanel;

    [Header("Scenes")]
    [SerializeField] string mainMenuSceneName = "MainMenu";

    void Awake()
    {
        Instance = this;

        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (RoomManager.Instance != null && RoomManager.Instance.IsTransitioning) return;

        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (CloseUpViewUI.Instance != null && CloseUpViewUI.Instance.IsOpen)
            return;

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
            return;
        }

        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (pausePanel == null) return;

        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
    }

    public void Resume()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void OnContinueClicked()
    {
        Resume();
    }

    public void OnSettingsClicked()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void OnSaveAndExitClicked()
    {
        SaveGame();
        Time.timeScale = 1f;
        IsPaused = false;

        if (PersistentRoot.Instance != null)
            Destroy(PersistentRoot.Instance.gameObject);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SaveGame()
    {
        GameObject player = PlayerMovement.FindInScene()?.gameObject;
        if (player == null || RoomManager.Instance == null)
        {
            Debug.LogWarning("Cannot save: missing Player or RoomManager.");
            return;
        }

        SaveData data = new SaveData
        {
            sceneName = RoomManager.Instance.CurrentSceneName,
            playerX = player.transform.position.x,
            playerY = player.transform.position.y,
            playerZ = player.transform.position.z
        };

        SanitySystem sanity = player.GetComponentInChildren<SanitySystem>();
        if (sanity != null)
        {
            data.hasSanity = true;
            data.sanityValue = sanity.CurrentSanity;
        }

        SaveSystem.Save(data);
    }
}
