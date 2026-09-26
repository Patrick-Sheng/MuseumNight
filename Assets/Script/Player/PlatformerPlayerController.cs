using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerPlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 8f;

    Rigidbody2D rb;
    Animator animator;
    int groundContacts;

    static readonly int HorizontalHash = Animator.StringToHash("horizontal");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void OnDisable()
    {
        groundContacts = 0;
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;

        float x = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        if (animator != null)
        {
            animator.SetBool("isMoving", x != 0);
            animator.SetFloat(HorizontalHash, x);
        }

        if (groundContacts > 0 && Input.GetButtonDown("Jump"))
            Bounce(jumpForce);
    }

    public void Bounce(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        groundContacts = 0;
    }

    void OnCollisionEnter2D(Collision2D collision) => CheckGrounded(collision);
    void OnCollisionStay2D(Collision2D collision) => CheckGrounded(collision);

    void CheckGrounded(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                groundContacts++;
                return;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (groundContacts > 0)
            groundContacts--;
    }
}
