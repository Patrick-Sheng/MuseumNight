using UnityEngine;
using UnityEngine.Events;

/// <summary>Point-based vision on the XY plane, with a prototype cone outline.</summary>
[RequireComponent(typeof(LineRenderer))]
public class GuardVision : MonoBehaviour
{
    [SerializeField] private Transform playerTarget;
    [SerializeField, Min(0.1f)] private float viewDistance = 5f;
    [SerializeField, Range(1f, 179f)] private float viewAngle = 70f;
    [Tooltip("Only solid walls/cover. Exclude the player, guard, and floor.")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private UnityEvent onPlayerDetected = new UnityEvent();
    [SerializeField] private bool logDetection = true;

    private const int ConeSegments = 48;
    private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[1];
    private readonly Vector3[] outlinePoints = new Vector3[ConeSegments + 2];
    private LineRenderer outline;

    public bool IsPlayerVisible { get; private set; }

    private void Awake()
    {
        outline = GetComponent<LineRenderer>();
        outline.useWorldSpace = true;
        outline.loop = true;
        outline.positionCount = outlinePoints.Length;
        outline.widthMultiplier = 0.04f;
        outline.sortingOrder = 4;
    }

    private void Start()
    {
        if (playerTarget == null)
        {
            Debug.LogError("GuardVision needs a Player Target from this scene.", this);
            enabled = false;
            return;
        }
        if (obstacleLayers.value == 0)
            Debug.LogWarning("GuardVision has no obstacle layers: walls will not block sight.", this);
    }

    private void LateUpdate()
    {
        // Run after patrol movement so the outline and detection use the same pose.
        bool visible = CanSeePlayer();
        bool justDetected = visible && !IsPlayerVisible;
        IsPlayerVisible = visible;
        DrawOutline();

        if (justDetected)
        {
            if (logDetection) Debug.Log("Guard detected the player.", this);
            onPlayerDetected.Invoke();
        }
    }

    private bool CanSeePlayer()
    {
        if (playerTarget == null) return false;
        Vector2 direction = playerTarget.position - transform.position;
        float distance = direction.magnitude;
        if (distance > viewDistance) return false;
        if (distance > 0.0001f && Vector2.Angle(transform.up, direction) > viewAngle * 0.5f)
            return false;

        return !TryGetObstacle(direction.normalized, distance, out _);
    }

    private bool TryGetObstacle(Vector2 direction, float distance, out RaycastHit2D hit)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleLayers);
        filter.useTriggers = false;
        int count = Physics2D.Raycast(transform.position, direction, filter, hitBuffer, distance);
        hit = count > 0 ? hitBuffer[0] : default;
        return count > 0;
    }

    private void DrawOutline()
    {
        Color colour = IsPlayerVisible ? Color.red : Color.yellow;
        outline.startColor = colour;
        outline.endColor = colour;
        outline.enabled = true;
        outlinePoints[0] = transform.position;

        for (int i = 0; i <= ConeSegments; i++)
        {
            float angle = -viewAngle * 0.5f + viewAngle * i / ConeSegments;
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * transform.up;
            float distance = TryGetObstacle(direction, viewDistance, out RaycastHit2D hit)
                ? hit.distance : viewDistance;
            outlinePoints[i + 1] = transform.position + (Vector3)(direction * distance);
        }
        outline.SetPositions(outlinePoints);
    }

    private void OnDisable()
    {
        IsPlayerVisible = false;
        if (outline != null) outline.enabled = false;
    }
}
