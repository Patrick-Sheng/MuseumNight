using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Owns stealth, pickup/escape, caught, and completed flow for the room.</summary>
public class StealthRoomController : MonoBehaviour
{
    [Tooltip("Optional override. When empty, finds PlayerMovement on the active Player-tagged object once in Awake.")]
    [SerializeField] private PlayerMovement playerMovement;
    // Only guards belonging to this room scene are managed, including initially inactive guards.
    private readonly List<GuardPatrol> guardPatrols = new List<GuardPatrol>();
    private readonly List<GuardVision> guardVisions = new List<GuardVision>();

    [Header("Escape after pickup")]
    [SerializeField] private ChaseEnemy enemyPrefab;
    [SerializeField] private Transform firstSpawnPoint;
    [SerializeField] private Transform secondSpawnPoint;
    [SerializeField] private bool keepGuardActiveDuringEscape = true;
    [Header("Persistent room retry")]
    [SerializeField] private string retryEntryId = "1";

    private Rigidbody2D playerBody;
    private PlayerInteract playerInteraction;
    private bool restarting;
    private string retryError;
    private ChaseEnemy firstEnemy;
    private ChaseEnemy secondEnemy;

    public bool IsCaught { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool HasObjective { get; private set; }
    public bool IsPlaying => isActiveAndEnabled && !IsCaught && !IsCompleted &&
        (RoomManager.Instance == null || !RoomManager.Instance.IsTransitioning);

    public bool IsPlayer(Collider2D other) => other.attachedRigidbody == playerBody;

    // Returns false without consuming the item if the escape setup is incomplete.
    public bool TryCollectObjective()
    {
        if (!IsPlaying || HasObjective) return false;
        if (enemyPrefab == null || firstSpawnPoint == null || secondSpawnPoint == null ||
            !enemyPrefab.IsConfigured)
        {
            Debug.LogError("Assign an active ChaseEnemy prefab (dynamic Rigidbody2D, solid CircleCollider2D) and two spawn points to the room.", this);
            return false;
        }

        HasObjective = true;
        firstEnemy = SpawnEnemy(firstSpawnPoint);
        secondEnemy = SpawnEnemy(secondSpawnPoint);
        if (!keepGuardActiveDuringEscape)
        {
            StopGuards();
        }
        return true;
    }

    private ChaseEnemy SpawnEnemy(Transform point)
    {
        Vector3 position = new Vector3(point.position.x, point.position.y, playerMovement.transform.position.z);
        ChaseEnemy enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(enemy.gameObject, gameObject.scene);
        enemy.Initialize(playerBody, this);
        return enemy;
    }

    private void Awake()
    {
        if (playerMovement == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerMovement = player.GetComponent<PlayerMovement>();
        }

        if (playerMovement == null)
        {
            Debug.LogError("StealthRoomController needs assigned Player Movement or an active Player-tagged object with PlayerMovement. Load Persistent before this room, or assign a test player.", this);
            enabled = false;
            return;
        }

        playerBody = playerMovement.GetComponent<Rigidbody2D>();
        playerInteraction = playerMovement.GetComponent<PlayerInteract>();

        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            guardPatrols.AddRange(root.GetComponentsInChildren<GuardPatrol>(true));
            guardVisions.AddRange(root.GetComponentsInChildren<GuardVision>(true));
        }
    }

    private void OnEnable()
    {
        foreach (GuardVision vision in guardVisions)
            if (vision != null) vision.AddDetectionListener(CatchPlayer);
    }

    private void OnDisable()
    {
        foreach (GuardVision vision in guardVisions)
            if (vision != null) vision.RemoveDetectionListener(CatchPlayer);
    }

    private void StopGuards()
    {
        foreach (GuardPatrol patrol in guardPatrols)
            if (patrol != null) patrol.enabled = false;
        foreach (GuardVision vision in guardVisions)
            if (vision != null) vision.enabled = false;
    }

    // Automatically connected to every GuardVision in this room scene.
    public void CatchPlayer()
    {
        if (!IsPlaying) return;
        IsCaught = true;
        StopRoom();
    }

    public void CompleteRoom()
    {
        if (!IsPlaying || !HasObjective) return;
        IsCompleted = true;
        StopRoom();
    }

    public bool TryExitToRoom(string sceneName, string entryId)
    {
        if (!IsPlaying || !HasObjective || PauseMenu.IsPaused) return false;
        RoomManager manager = RoomManager.Instance;
        if (manager == null)
        {
            Debug.LogError("Room transitions require Persistent/RoomManager. Leave the exit destination empty for standalone completion.", this);
            return false;
        }
        if (!manager.TryGoToRoom(sceneName, entryId, true, HandleTransitionFinished)) return false;
        IsCompleted = true;
        restarting = true;
        StopRoom();
        return true;
    }

    private void HandleTransitionFinished(bool succeeded)
    {
        if (this == null || succeeded) return;
        restarting = false;
        retryError = "Transition failed. Check the Console and entry ID. Press R to retry this room.";
        StopRoom();
    }

    private void StopRoom()
    {
        playerMovement.enabled = false;
        playerMovement.isMoving = false;
        if (playerInteraction != null) playerInteraction.enabled = false;
        // Removing the body from simulation also cancels pending movement/contact motion.
        playerBody.simulated = false;
        StopGuards();
        if (firstEnemy != null) firstEnemy.StopChasing();
        if (secondEnemy != null) secondEnemy.StopChasing();
    }

    private void Update()
    {
        if ((IsCaught || IsCompleted) && Input.GetKeyDown(KeyCode.R)) RetryRoom();
    }

    public void RetryRoom()
    {
        if (!isActiveAndEnabled || (!IsCaught && !IsCompleted) || restarting || PauseMenu.IsPaused) return;

        if (RoomManager.Instance != null)
        {
            restarting = RoomManager.Instance.TryGoToRoom(gameObject.scene.name, retryEntryId,
                true, HandleTransitionFinished);
            if (!restarting) retryError = "Cannot retry now. Check the Console and room scene settings.";
            return;
        }

        // Use this object's scene, avoiding ambiguity if another scene is active.
        string scenePath = gameObject.scene.path;
        if (!Application.CanStreamedLevelBeLoaded(scenePath))
        {
            retryError = "Enable this saved scene in the Build Profiles scene list, then retry.";
            Debug.LogError(retryError, this);
            return;
        }

        restarting = true;
        SceneManager.LoadScene(scenePath, LoadSceneMode.Single);
    }

    private void OnGUI()
    {
        if (RoomManager.Instance != null && RoomManager.Instance.IsTransitioning) return;
        if (IsPlaying)
        {
            if (HasObjective)
            {
                float escapeScale = Mathf.Clamp(Screen.height / 720f, 0.75f, 2f);
                GUIStyle escapeStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = Mathf.RoundToInt(24f * escapeScale),
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
                float escapeWidth = Mathf.Min(600f * escapeScale, Screen.width - 20f);
                GUI.Box(new Rect((Screen.width - escapeWidth) * 0.5f, 20f,
                    escapeWidth, 65f * escapeScale), "Item collected — reach the exit!", escapeStyle);
            }
            return;
        }

        // Temporary prototype feedback; replace with the game's Canvas UI later.
        float scale = Mathf.Clamp(Screen.height / 720f, 0.75f, 2f);
        float width = Mathf.Min(640f * scale, Screen.width - 20f);
        float height = Mathf.Min(240f * scale, Screen.height - 20f);
        Rect panel = new Rect((Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f, width, height);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(40f * scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(24f * scale),
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };

        GUI.Box(panel, GUIContent.none);
        GUI.Label(new Rect(panel.x + 15f, panel.y + height * 0.1f,
            width - 30f, height * 0.3f), IsCompleted ? "Escaped!" : "Caught!", titleStyle);
        GUI.Label(new Rect(panel.x + 15f, panel.y + height * 0.45f,
            width - 30f, height * 0.45f),
            retryError ?? (IsCompleted ? "Room complete. Press R to play again." : "Press R to retry the room."), messageStyle);
    }
}
