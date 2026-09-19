using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private string startingSceneName;
    [SerializeField] private string startingEntryId = "1";

    private string currentSceneName;
    private bool isTransitioning = false;

    public string CurrentSceneName => currentSceneName;
    public bool IsTransitioning => isTransitioning;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        SaveData save = SaveSystem.Load();
        if (save != null)
        {
            StartCoroutine(TransitionRoutine(save.sceneName, null, save));
        }
        else
        {
            StartCoroutine(TransitionRoutine(startingSceneName, startingEntryId, null));
        }
    }

    public void GoToRoom(string sceneName, string entryPointId)
    {
        TryGoToRoom(sceneName, entryPointId);
    }

    public bool TryGoToRoom(string sceneName, string entryPointId,
        bool restorePlayerControl = false, System.Action<bool> onFinished = null)
    {
        if (isTransitioning || PauseMenu.IsPaused) return false;
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("RoomManager: destination must be an enabled scene in Build Profiles: " + sceneName, this);
            return false;
        }
        StartCoroutine(TransitionRoutine(sceneName, entryPointId, null, restorePlayerControl, onFinished));
        return true;
    }

    private IEnumerator TransitionRoutine(string sceneName, string entryPointId, SaveData loadedSave,
        bool restorePlayerControl = false, System.Action<bool> onFinished = null)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("RoomManager cannot load scene: " + sceneName, this);
            onFinished?.Invoke(false);
            yield break;
        }

        isTransitioning = true;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("RoomManager requires the persistent Player before loading a room.", this);
            isTransitioning = false;
            onFinished?.Invoke(false);
            yield break;
        }

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        PlayerInteract interaction = player.GetComponent<PlayerInteract>();
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        bool movementWasEnabled = movement != null && movement.enabled;
        bool interactionWasEnabled = interaction != null && interaction.enabled;
        bool bodyWasSimulated = body != null && body.simulated;
        if (movement != null) { movement.enabled = false; movement.isMoving = false; }
        if (interaction != null) { interaction.ClearInteraction(); interaction.enabled = false; }
        if (body != null) { body.linearVelocity = Vector2.zero; body.simulated = false; }
        Animator animator = player.GetComponent<Animator>();
        if (animator != null) animator.SetBool("isMoving", false);

        yield return StartCoroutine(FadeTo(1f));

        // Keep the old room until the destination and its entry are validated.
        // Capture the loaded Scene directly: retries briefly have two scenes with the same name.
        Scene oldScene = string.IsNullOrEmpty(currentSceneName) ? default : SceneManager.GetSceneByName(currentSceneName);
        Scene loadedScene = default;
        void CaptureLoadedScene(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive && (scene.name == sceneName || scene.path == sceneName))
                loadedScene = scene;
        }
        SceneManager.sceneLoaded += CaptureLoadedScene;
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        SceneManager.sceneLoaded -= CaptureLoadedScene;
        EntryPoint entry = loadedScene.IsValid() ? FindEntryPointInScene(loadedScene, entryPointId) : null;
        if (!loadedScene.IsValid() || (loadedSave == null && entry == null))
        {
            Debug.LogError("RoomManager: destination or EntryPoint is missing: " + sceneName + " / " + entryPointId, this);
            if (loadedScene.IsValid()) yield return SceneManager.UnloadSceneAsync(loadedScene);
            yield return StartCoroutine(FadeTo(0f));
            if (movement != null) movement.enabled = movementWasEnabled;
            if (interaction != null) interaction.enabled = interactionWasEnabled;
            if (body != null) body.simulated = bodyWasSimulated;
            isTransitioning = false;
            onFinished?.Invoke(false);
            yield break;
        }

        if (oldScene.IsValid() && oldScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(oldScene);
        currentSceneName = loadedScene.name;
        SceneManager.SetActiveScene(loadedScene);

        if (loadedSave != null)
        {
            if (player != null)
                player.transform.position = new Vector3(loadedSave.playerX, loadedSave.playerY, loadedSave.playerZ);

            if (loadedSave.hasSanity)
            {
                SanitySystem sanity = player != null ? player.GetComponentInChildren<SanitySystem>() : null;
                if (sanity != null)
                    sanity.SetSanity(loadedSave.sanityValue);
            }
        }
        else
        {
            if (entry != null && player != null)
                player.transform.position = entry.transform.position;
        }

        if (body != null)
        {
            body.position = player.transform.position;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
        Physics2D.SyncTransforms();

        yield return StartCoroutine(FadeTo(0f));

        if (movement != null) movement.enabled = restorePlayerControl || movementWasEnabled;
        if (interaction != null) interaction.enabled = restorePlayerControl || interactionWasEnabled;
        if (body != null) body.simulated = restorePlayerControl || bodyWasSimulated;
        isTransitioning = false;
        onFinished?.Invoke(true);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }

    private EntryPoint FindEntryPointInScene(Scene scene, string id)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            EntryPoint[] points = root.GetComponentsInChildren<EntryPoint>();
            foreach (var p in points)
            {
                if (p.entryId == id)
                    return p;
            }
        }
        return null;
    }
}
