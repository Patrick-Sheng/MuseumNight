using UnityEngine;

/// <summary>Animates a visual child without changing its owner's movement or rotation.</summary>
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class FourDirectionAnimator : MonoBehaviour
{
    [Tooltip("The moving gameplay object. Defaults to this visual's parent. Do not assign the visual itself.")]
    [SerializeField] private Transform movementRoot;
    [Tooltip("Optional: use this object's local +Y as facing, even while standing still. Assign the guard root for its look-around sweep. Leave empty to face the direction of movement.")]
    [SerializeField] private Transform facingSource;
    [SerializeField] private bool keepUpright = true;
    [Tooltip("Movement below this speed (world units per second) counts as idle.")]
    [SerializeField, Min(0f)] private float minimumMoveSpeed = 0.01f;

    private Animator animator;
    private Vector2 previousPosition;
    private Vector2 lastDirection = Vector2.down;

    private static readonly int Horizontal = Animator.StringToHash("horizontal");
    private static readonly int Vertical = Animator.StringToHash("vertical");
    private static readonly int LastHorizontal = Animator.StringToHash("lastHorizontal");
    private static readonly int LastVertical = Animator.StringToHash("lastVertical");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (movementRoot == null) movementRoot = transform.parent;

        // The visual may counter-rotate; it must never be its own movement/facing source.
        if (movementRoot == null || movementRoot.IsChildOf(transform) ||
            (facingSource != null && facingSource.IsChildOf(transform)))
        {
            Debug.LogError("FourDirectionAnimator needs a movement root outside its visual hierarchy, and an optional facing source outside that hierarchy. Put it on the Body child and assign the gameplay root.", this);
            enabled = false;
            return;
        }
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("FourDirectionAnimator needs an Animator Controller with the five directional movement parameters.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (movementRoot == null || animator == null) return;
        previousPosition = movementRoot.position;
        UpdateAnimation(facingSource != null ? (Vector2)facingSource.up : lastDirection, false);
    }

    private void LateUpdate()
    {
        if (movementRoot == null) return;

        Vector2 position = movementRoot.position;
        Vector2 displacement = position - previousPosition;
        previousPosition = position;

        // Only the artwork stays upright. The guard root still rotates its vision cone.
        if (keepUpright) transform.rotation = Quaternion.identity;
        if (Time.deltaTime <= 0f) return;

        float minimumDistance = minimumMoveSpeed * Time.deltaTime;
        bool moving = displacement.sqrMagnitude > minimumDistance * minimumDistance;
        Vector2 direction = facingSource != null ? (Vector2)facingSource.up :
            (moving ? displacement : lastDirection);
        UpdateAnimation(direction, moving);
    }

    private void UpdateAnimation(Vector2 direction, bool moving)
    {
        // Supply exactly one cardinal direction to avoid ambiguous diagonal sprite blends.
        if (direction.sqrMagnitude > 0.000001f)
            lastDirection = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? new Vector2(Mathf.Sign(direction.x), 0f)
                : new Vector2(0f, Mathf.Sign(direction.y));

        animator.SetFloat(Horizontal, moving ? lastDirection.x : 0f);
        animator.SetFloat(Vertical, moving ? lastDirection.y : 0f);
        animator.SetFloat(LastHorizontal, lastDirection.x);
        animator.SetFloat(LastVertical, lastDirection.y);
        animator.SetBool(IsMoving, moving);
    }

    private void OnDisable()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetBool(IsMoving, false);
    }
}
