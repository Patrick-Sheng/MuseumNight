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

    private string currentSceneName;
    private bool isTransitioning = false;

    public string CurrentSceneName => currentSceneName;

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
            StartCoroutine(TransitionRoutine(startingSceneName, "default_spawn", null));
        }
    }

    public void GoToRoom(string sceneName, string entryPointId)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName, entryPointId, null));
    }

    private IEnumerator TransitionRoutine(string sceneName, string entryPointId, SaveData loadedSave)
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeTo(1f));

        if (!string.IsNullOrEmpty(currentSceneName))
            yield return SceneManager.UnloadSceneAsync(currentSceneName);

        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        currentSceneName = sceneName;

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        GameObject player = GameObject.FindGameObjectWithTag("Player");

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
            EntryPoint entry = FindEntryPointInScene(loadedScene, entryPointId);
            if (entry != null && player != null)
                player.transform.position = entry.transform.position;
        }

        yield return StartCoroutine(FadeTo(0f));

        isTransitioning = false;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
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