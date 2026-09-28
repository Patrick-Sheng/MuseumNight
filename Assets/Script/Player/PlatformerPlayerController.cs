using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerPlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 13f;

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;
    int groundContacts;
    [SerializeField, Min(0.1f)] float footstepInterval = 0.38f;
    float nextFootstepTime;
    bool nextFootIsLeft;
    float lastFacing = 1f;

    static readonly int HorizontalHash = Animator.StringToHash("horizontal");
    static readonly int VerticalHash = Animator.StringToHash("vertical");
    static readonly int LastHorizontalHash = Animator.StringToHash("lastHorizontal");
    static readonly int LastVerticalHash = Animator.StringToHash("lastVertical");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void OnEnable()
    {
        // The Animator is shared with the top-down PlayerMovement, which can leave
        // "vertical"/"lastVertical" non-zero. Reset them so the platformer never
        // starts (or gets stuck) on an up/down animation.
        if (animator != null)
        {
            animator.SetFloat(VerticalHash, 0f);
            animator.SetFloat(LastHorizontalHash, lastFacing);
            animator.SetFloat(LastVerticalHash, 0f);
        }
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
        if (x != 0f && groundContacts > 0 && Time.time >= nextFootstepTime)
        {
            GameAudio.Play(nextFootIsLeft ? GameAudio.Cue.FootLeft : GameAudio.Cue.FootRight);
            nextFootIsLeft = !nextFootIsLeft;
            nextFootstepTime = Time.time + footstepInterval;
        }
        else if (x == 0f)
        {
            nextFootstepTime = Time.time;
        }

        if (x != 0) lastFacing = Mathf.Sign(x);

        if (animator != null)
        {
            animator.SetBool("isMoving", x != 0);
            animator.SetFloat(HorizontalHash, x);
            animator.SetFloat(VerticalHash, 0f);
            animator.SetFloat(LastHorizontalHash, lastFacing);
            animator.SetFloat(LastVerticalHash, 0f);
        }

        if (spriteRenderer != null && x != 0)
            spriteRenderer.flipX = x < 0;

        if (groundContacts > 0 && Input.GetButtonDown("Jump"))
            Bounce(jumpForce);
    }

    public void Bounce(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        groundContacts = 0;
        GameAudio.Play(GameAudio.Cue.Jump);
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
