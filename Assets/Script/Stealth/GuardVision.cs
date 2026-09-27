using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

/// <summary>Point-based vision with an outline or a soft 2D light cone.</summary>
[RequireComponent(typeof(LineRenderer))]
public class GuardVision : MonoBehaviour
{
    [Tooltip("Optional override. When empty, finds the active Player-tagged object once at startup.")]
    [SerializeField] private Transform playerTarget;
    [SerializeField, Min(0.1f)] private float viewDistance = 5f;
    [SerializeField, Range(1f, 179f)] private float viewAngle = 70f;
    [Tooltip("Only solid walls/cover. Exclude the player, guard, and floor.")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private UnityEvent onPlayerDetected = new UnityEvent();
    [SerializeField] private bool logDetection = true;

    [Header("Cone appearance")]
    [Tooltip("Use the same soft Light2D cone as the player flashlight. Cosmetic light does not clip to physics walls; detection still does.")]
    [SerializeField] private bool useGradientCone;
    [SerializeField] private Color coneColor = new Color(1f, 0.04f, 0.02f, 1f);
    [SerializeField, Min(0f)] private float coneIntensity = 2f;
    [SerializeField, Range(0f, 1f)] private float innerAngleRatio = 0.5f;
    [SerializeField, Range(0f, 1f)] private float coneFalloff = 0.5f;
    private Light2D coneLight;

    private const int ConeSegments = 48;
    private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[1];
    private readonly Vector3[] outlinePoints = new Vector3[ConeSegments + 2];
    private LineRenderer outline;

    public bool IsPlayerVisible { get; private set; }

    public void AddDetectionListener(UnityAction listener) => onPlayerDetected.AddListener(listener);
    public void RemoveDetectionListener(UnityAction listener) => onPlayerDetected.RemoveListener(listener);

    private void Awake()
    {
        outline = GetComponent<LineRenderer>();
        outline.useWorldSpace = true;
        outline.loop = true;
        outline.positionCount = outlinePoints.Length;
        outline.widthMultiplier = 0.04f;
        outline.sortingOrder = 4;
        if (useGradientCone)
        {
            GameObject visual = new GameObject("Guard Cone Light");
            visual.transform.SetParent(transform, false);
            coneLight = visual.AddComponent<Light2D>();
            coneLight.lightType = Light2D.LightType.Point;
            coneLight.targetSortingLayers = new[] { SortingLayer.NameToID("Default") };
            coneLight.enabled = false;
            outline.enabled = false;
        }
    }

    private void Start()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }

        if (playerTarget == null)
        {
            Debug.LogError("GuardVision needs an assigned Player Target or an active Player-tagged object. Load Persistent before this room, or assign a test player.", this);
            enabled = false;
            return;
        }
        if (obstacleLayers.value == 0)
            Debug.LogWarning("GuardVision has no obstacle layers: walls will not block sight.", this);
    }

    private void LateUpdate()
    {
        if (PauseMenu.IsPaused) return;
        if (RoomManager.Instance != null && RoomManager.Instance.IsTransitioning) return;
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
        if (coneLight != null)
        {
            outline.enabled = false;
            coneLight.color = coneColor;
            coneLight.intensity = coneIntensity;
            coneLight.pointLightInnerRadius = 0f;
            coneLight.pointLightOuterRadius = viewDistance;
            coneLight.pointLightInnerAngle = viewAngle * innerAngleRatio;
            coneLight.pointLightOuterAngle = viewAngle;
            coneLight.falloffIntensity = coneFalloff;
            coneLight.enabled = true;
            return;
        }
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
        if (coneLight != null) coneLight.enabled = false;
    }
}
