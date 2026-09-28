using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Persistent";
    [SerializeField] GameObject settingsPanel;
    [SerializeField] TMP_Text startButtonLabel;
    [SerializeField] private CanvasGroup startButton;
    [SerializeField] private Image background;
    [SerializeField] private Sprite crackedBackground;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip crackSound;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField, Range(0f, 1f)] private float menuMusicVolume = 0.35f;

    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float holdDuration = 1f;

    private bool isStarting = false;

    void Start()
    {
        if (menuMusic != null)
        {
            AudioSource musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = menuMusicVolume;
            musicSource.Play();
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (startButtonLabel != null)
            startButtonLabel.text = SaveSystem.HasSave() ? "Continue" : "Start";
    }

    private IEnumerator StartSequence()
    {
        // Stop button interaction
        startButton.interactable = false;
        startButton.blocksRaycasts = false;

        // Fade Start button + its Text
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            startButton.alpha =
                Mathf.Lerp(1f, 0f, time / fadeDuration);

            yield return null;
        }

        startButton.alpha = 0f;

        // Change background
        background.sprite = crackedBackground;

        // Play crack sound
        if (audioSource != null && crackSound != null)
        {
            audioSource.PlayOneShot(crackSound);
        }

        // Keep broken mirror visible
        yield return new WaitForSeconds(holdDuration);

        // Load game
        // Only request story intro for a fresh start flow.
        GameStartContext.SetStoryIntroForNextLoad(!SaveSystem.HasSave());
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnStartClicked()
    {
        if (isStarting)
            return;
        
        isStarting = true;
        StartCoroutine(StartSequence());
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
