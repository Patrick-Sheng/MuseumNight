using UnityEngine;

/// <summary>A predictable patrol on the XY plane. Local +Y is the guard's forward direction.</summary>
public class GuardPatrol : MonoBehaviour
{
    [Tooltip("Optional explicit route. When empty, use the direct children of Route Root in Hierarchy order.")]
    [SerializeField] private Transform[] waypoints;
    [Tooltip("Stationary route container, usually a sibling of this moving Guard inside a shared prefab root.")]
    [SerializeField] private Transform routeRoot;
    [SerializeField, Min(0.1f)] private float moveSpeed = 2f;
    [SerializeField, Min(1f)] private float turnSpeed = 180f;
    [SerializeField, Min(0f)] private float lookDuration = 3f;
    [SerializeField, Range(0f, 180f)] private float lookAngle = 60f;

    private int waypointIndex;
    private bool looking;
    private float lookElapsed;
    private float arrivalAngle;

    public Vector2 FacingDirection => transform.up;

    private void Start()
    {
        if ((waypoints == null || waypoints.Length == 0) && routeRoot != null)
        {
            waypoints = new Transform[routeRoot.childCount];
            for (int i = 0; i < waypoints.Length; i++)
                waypoints[i] = routeRoot.GetChild(i);
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("GuardPatrol needs explicit waypoints or a Route Root containing waypoint children.", this);
            enabled = false;
            return;
        }

        foreach (Transform point in waypoints)
        {
            if (point == null || point == transform || point.IsChildOf(transform))
            {
                Debug.LogError("Waypoints must be assigned and outside the moving Guard. Put Guard and PatrolRoute beside each other under a stationary parent.", this);
                enabled = false;
                return;
            }
        }
    }

    private void Update()
    {
        if (looking)
        {
            lookElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(lookElapsed / lookDuration);
            // One sweep: centre -> left -> right -> centre.
            float angle = arrivalAngle + Mathf.Sin(progress * Mathf.PI * 2f) * lookAngle;
            TurnTowards(angle);

            if (progress >= 1f)
            {
                looking = false;
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
            }
            return;
        }

        Transform point = waypoints[waypointIndex];
        if (point == null)
        {
            Debug.LogError("A GuardPatrol waypoint was removed during play.", this);
            enabled = false;
            return;
        }

        Vector3 target = new Vector3(point.position.x, point.position.y, transform.position.z);
        Vector2 direction = target - transform.position;
        if (direction.sqrMagnitude > 0.000001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            TurnTowards(angle);
        }

        // This prototype follows clear, designer-placed routes; it does not avoid obstacles.
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        if ((transform.position - target).sqrMagnitude <= 0.000001f)
        {
            if (lookDuration > 0f)
            {
                looking = true;
                lookElapsed = 0f;
                arrivalAngle = transform.eulerAngles.z;
            }
            else
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
            }
        }
    }

    private void TurnTowards(float angle)
    {
        Quaternion target = Quaternion.Euler(0f, 0f, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
        bool explicitRoute = waypoints != null && waypoints.Length > 0;
        int count = explicitRoute ? waypoints.Length : (routeRoot != null ? routeRoot.childCount : 0);
        if (count == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < count; i++)
        {
            Transform point = explicitRoute ? waypoints[i] : routeRoot.GetChild(i);
            Transform next = explicitRoute ? waypoints[(i + 1) % count] : routeRoot.GetChild((i + 1) % count);
            if (point == null) continue;
            Gizmos.DrawWireSphere(point.position, 0.15f);
            if (next != null) Gizmos.DrawLine(point.position, next.position);
        }
    }
}
