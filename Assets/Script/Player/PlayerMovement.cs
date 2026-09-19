using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed = 5f;

    Animator animator;
    Vector2 inputVector;
    public Vector2 lastDirection = Vector2.down;

    Rigidbody2D rb;
    public bool isMoving;
    public Vector2 InputDirection => inputDirection;

    static readonly int HorizontalHash = Animator.StringToHash("horizontal");
    static readonly int VerticalHash   = Animator.StringToHash("vertical");
    static readonly int LastHorizontalHash = Animator.StringToHash("lastHorizontal");
    static readonly int LastVerticalHash   = Animator.StringToHash("lastVertical");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    Vector2 inputDirection;

    void Update()
    {
        Debug.Log("I am moving! " + isMoving);

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(x, y).normalized;
        isMoving = inputDirection != Vector2.zero;

        animator.SetBool("isMoving", isMoving);

        if (inputDirection != Vector2.zero)
            lastDirection = inputDirection;

        animator.SetFloat(HorizontalHash,     x);
        animator.SetFloat(VerticalHash,       y);
        animator.SetFloat(LastHorizontalHash, lastDirection.x);
        animator.SetFloat(LastVerticalHash,   lastDirection.y);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + inputDirection * (speed * Time.fixedDeltaTime));
    }
}