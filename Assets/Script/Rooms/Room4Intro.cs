using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

/// <summary>Walks the persistent player through the Room 4 introduction.</summary>
public class Room4Intro : MonoBehaviour
{
    [SerializeField] private PlayableDirector introDirector;
    [SerializeField] private Transform walkUp;
    [SerializeField] private Transform walkLeft;
    [SerializeField] private Room4CameraZoom roomCamera;
    [SerializeField] private CanvasGroup borders;
    [SerializeField] private Room4Outcome outcome;
    [SerializeField, Min(0.1f)] private float walkSpeed = 2f;
    [SerializeField, Min(0f)] private float statuePause = 0.8f;
    [SerializeField, Min(0f)] private float revealTime = 10f;
    [SerializeField, Min(0f)] private float borderFadeDuration = 0.8f;

    private PlayerMovement movement;
    private PlayerInteract interaction;
    private Rigidbody2D body;
    private Animator animator;
    private bool movementWasEnabled;
    private bool interactionWasEnabled;
    private bool bodyWasSimulated;
    private bool controlsCaptured;
    private bool started;

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene() == gameObject.scene)
            Begin();
    }

    private void OnActiveSceneChanged(Scene previous, Scene current)
    {
        if (current == gameObject.scene)
            Begin();
    }

    private void Begin()
    {
        if (started) return;
        started = true;
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        // The room becomes active before RoomManager finishes its fade and restores controls.
        while (RoomManager.Instance != null && RoomManager.Instance.IsTransitioning)
            yield return null;

        if (introDirector == null || walkUp == null || walkLeft == null || roomCamera == null || borders == null || outcome == null)
        {
            Debug.LogError("Room4Intro needs its Timeline, walk points, camera, borders, and outcome assigned.", this);
            yield break;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Room4Intro needs the persistent Player.", this);
            yield break;
        }

        movement = player.GetComponent<PlayerMovement>();
        interaction = player.GetComponent<PlayerInteract>();
        body = player.GetComponent<Rigidbody2D>();
        animator = player.GetComponent<Animator>();
        movementWasEnabled = movement != null && movement.enabled;
        interactionWasEnabled = interaction != null && interaction.enabled;
        bodyWasSimulated = body != null && body.simulated;
        controlsCaptured = true;

        if (movement != null) movement.enabled = false;
        if (interaction != null) { interaction.ClearInteraction(); interaction.enabled = false; }
        if (body != null) { body.linearVelocity = Vector2.zero; body.simulated = false; }

        borders.alpha = 1f;
        introDirector.time = 0;
        introDirector.Play();
        yield return MoveTo(player.transform, walkUp.position);
        SetWalking(Vector2.zero);
        yield return new WaitForSeconds(statuePause);
        yield return MoveTo(player.transform, walkLeft.position);
        SetWalking(Vector2.zero);

        while (introDirector != null && introDirector.time < revealTime && introDirector.time < introDirector.duration - 0.02)
            yield return null;
        FaceRight();
        roomCamera.RevealRoom();
        yield return FadeBorders();

        while (introDirector != null && introDirector.time < introDirector.duration - 0.02)
            yield return null;

        outcome.PlayOutcome();
    }

    private IEnumerator FadeBorders()
    {
        float startAlpha = borders.alpha;
        float elapsed = 0f;
        while (elapsed < borderFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            borders.alpha = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / borderFadeDuration));
            yield return null;
        }
        borders.alpha = 0f;
    }

    private IEnumerator MoveTo(Transform player, Vector3 destination)
    {
        while (Vector2.Distance(player.position, destination) > 0.02f)
        {
            Vector2 direction = ((Vector2)(destination - player.position)).normalized;
            SetWalking(direction);
            Vector2 next = Vector2.MoveTowards(player.position, destination, walkSpeed * Time.deltaTime);
            player.position = new Vector3(next.x, next.y, player.position.z);
            yield return null;
        }
        player.position = new Vector3(destination.x, destination.y, player.position.z);
    }

    private void SetWalking(Vector2 direction)
    {
        bool walking = direction != Vector2.zero;
        if (movement != null)
        {
            movement.isMoving = walking;
            if (walking) movement.lastDirection = direction;
        }
        if (animator == null) return;
        animator.SetBool("isMoving", walking);
        if (!walking) return;
        animator.SetFloat("horizontal", direction.x);
        animator.SetFloat("vertical", direction.y);
        animator.SetFloat("lastHorizontal", direction.x);
        animator.SetFloat("lastVertical", direction.y);
    }

    private void FaceRight()
    {
        if (movement != null)
        {
            movement.isMoving = false;
            movement.lastDirection = Vector2.right;
        }
        if (animator == null) return;
        animator.SetBool("isMoving", false);
        animator.SetFloat("horizontal", 0f);
        animator.SetFloat("vertical", 0f);
        animator.SetFloat("lastHorizontal", 1f);
        animator.SetFloat("lastVertical", 0f);
    }

    private void RestoreControls()
    {
        if (!controlsCaptured) return;
        SetWalking(Vector2.zero);
        if (body != null)
        {
            body.position = body.transform.position;
            body.linearVelocity = Vector2.zero;
            body.simulated = bodyWasSimulated;
        }
        if (movement != null) movement.enabled = movementWasEnabled;
        if (interaction != null) interaction.enabled = interactionWasEnabled;
        controlsCaptured = false;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        RestoreControls();
    }
}
