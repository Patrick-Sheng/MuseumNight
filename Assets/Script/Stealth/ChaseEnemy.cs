using UnityEngine;

/// <summary>Direct pursuit with wall collisions. Does not find routes around obstacles.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class ChaseEnemy : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float moveSpeed = 3.5f;

    private Rigidbody2D body;
    private Rigidbody2D target;
    private System.Action onCaught;
    private System.Func<bool> canChase;

    public bool IsConfigured => gameObject.activeSelf && enabled &&
        GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Dynamic &&
        GetComponent<Rigidbody2D>().simulated &&
        GetComponent<CircleCollider2D>().enabled && !GetComponent<CircleCollider2D>().isTrigger;

    private void Reset()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        GetComponent<CircleCollider2D>().isTrigger = false;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Initialize(Rigidbody2D player, StealthRoomController owner)
    {
        Initialize(player, () => { if (owner != null) owner.CatchPlayer(); },
            () => owner != null && owner.IsPlaying);
    }

    public void Initialize(Rigidbody2D player, System.Action caught, System.Func<bool> chaseAllowed)
    {
        target = player;
        onCaught = caught;
        canChase = chaseAllowed;
    }

    private void FixedUpdate()
    {
        if (target == null || canChase == null || !canChase())
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 position = Vector2.MoveTowards(body.position, target.position, moveSpeed * Time.fixedDeltaTime);
        body.MovePosition(position);
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckPlayerContact(collision);
    private void OnCollisionStay2D(Collision2D collision) => CheckPlayerContact(collision);

    private void CheckPlayerContact(Collision2D collision)
    {
        if (isActiveAndEnabled && target != null && canChase != null && canChase() &&
            collision.collider.attachedRigidbody == target)
            onCaught?.Invoke();
    }

    public void StopChasing()
    {
        body.linearVelocity = Vector2.zero;
        body.simulated = false;
        enabled = false;
    }
}
