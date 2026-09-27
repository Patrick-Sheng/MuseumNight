using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Frames Room 4 closer, then restores the persistent camera on exit.</summary>
public class Room4CameraZoom : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float closeSize = 6.5f;
    [SerializeField, Min(0f)] private float zoomDuration = 0.6f;
    [SerializeField, Min(0f)] private float followSpeed = 5f;

    private Camera mainCamera;
    private Transform player;
    private float originalSize;
    private Vector3 originalPosition;
    private bool zoomStarted;
    private bool revealing;
    private Coroutine zoomRoutine;

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene() == gameObject.scene)
            BeginZoom();
    }

    private void OnActiveSceneChanged(Scene previous, Scene current)
    {
        if (current == gameObject.scene)
            BeginZoom();
    }

    private void BeginZoom()
    {
        if (zoomStarted) return;

        mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic)
        {
            Debug.LogWarning("Room4CameraZoom needs the persistent orthographic Main Camera.", this);
            return;
        }

        originalSize = mainCamera.orthographicSize;
        originalPosition = mainCamera.transform.position;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
        zoomStarted = true;
        zoomRoutine = StartCoroutine(ZoomTo(closeSize));
    }

    private void LateUpdate()
    {
        if (!zoomStarted || mainCamera == null) return;

        Vector3 target = revealing || player == null
            ? originalPosition
            : new Vector3(player.position.x, player.position.y, originalPosition.z);
        float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target, t);
    }

    public void RevealRoom()
    {
        if (!zoomStarted || revealing) return;
        revealing = true;
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        zoomRoutine = StartCoroutine(ZoomTo(originalSize));
    }

    private IEnumerator ZoomTo(float targetSize)
    {
        float startSize = mainCamera.orthographicSize;
        if (zoomDuration <= 0f)
        {
            mainCamera.orthographicSize = targetSize;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / zoomDuration);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }
        mainCamera.orthographicSize = targetSize;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        if (zoomStarted && mainCamera != null)
        {
            mainCamera.orthographicSize = originalSize;
            mainCamera.transform.position = originalPosition;
        }
        zoomStarted = false;
        revealing = false;
    }
}
