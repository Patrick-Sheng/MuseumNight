using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Owns the playing, caught, and completed flow for a single-guard room.</summary>
public class StealthRoomController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GuardPatrol guardPatrol;
    [SerializeField] private GuardVision guardVision;

    private Rigidbody2D playerBody;
    private PlayerInteract playerInteraction;
    private bool restarting;
    private string retryError;

    public bool IsCaught { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsPlaying => isActiveAndEnabled && !IsCaught && !IsCompleted;

    private void Awake()
    {
        if (playerMovement == null || guardPatrol == null || guardVision == null)
        {
            Debug.LogError("StealthRoomController needs Player Movement, Guard Patrol, and Guard Vision references.", this);
            enabled = false;
            return;
        }

        playerBody = playerMovement.GetComponent<Rigidbody2D>();
        playerInteraction = playerMovement.GetComponent<PlayerInteract>();
    }

    // Connect GuardVision's On Player Detected event to this method in the scene.
    public void CatchPlayer()
    {
        if (!IsPlaying) return;
        IsCaught = true;
        StopRoom();
    }

    public void CompleteRoom()
    {
        if (!IsPlaying) return;
        IsCompleted = true;
        StopRoom();
    }

    private void StopRoom()
    {
        playerMovement.enabled = false;
        playerMovement.isMoving = false;
        if (playerInteraction != null) playerInteraction.enabled = false;
        // Removing the body from simulation also cancels pending movement/contact motion.
        playerBody.simulated = false;
        guardPatrol.enabled = false;
        guardVision.enabled = false;
    }

    private void Update()
    {
        if ((IsCaught || IsCompleted) && Input.GetKeyDown(KeyCode.R)) RetryRoom();
    }

    public void RetryRoom()
    {
        if (!isActiveAndEnabled || IsPlaying || restarting) return;

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
        if (IsPlaying) return;

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
            width - 30f, height * 0.3f), IsCompleted ? "Item secured!" : "Caught!", titleStyle);
        GUI.Label(new Rect(panel.x + 15f, panel.y + height * 0.45f,
            width - 30f, height * 0.45f),
            retryError ?? (IsCompleted ? "Room complete. Press R to play again." : "Press R to retry the room."), messageStyle);
    }
}
