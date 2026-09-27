using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LightProjectile : MonoBehaviour
{
    [SerializeField] float speed = 12f;

    Rigidbody2D rb;
    Vector2 direction;

    public void Init(Vector2 startDirection)
    {
        direction = startDirection.normalized;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Mirror mirror = other.GetComponent<Mirror>();
        if (mirror != null)
        {
            direction = Vector2.Reflect(direction, (Vector2)mirror.transform.up).normalized;
            Debug.Log($"Projectile bounced off {mirror.name}, new direction {direction}", this);
            return;
        }

        LightReceiver receiver = other.GetComponent<LightReceiver>();
        if (receiver != null)
        {
            receiver.Illuminate();
            Debug.Log($"Projectile reached {receiver.name} - puzzle solved", this);
            Destroy(gameObject);
            return;
        }

        // Anything else solid (walls, platforms) ends the projectile's flight.
        if (!other.isTrigger)
        {
            Debug.Log($"Projectile hit solid collider {other.name}, destroying", this);
            Destroy(gameObject);
        }
    }
}
