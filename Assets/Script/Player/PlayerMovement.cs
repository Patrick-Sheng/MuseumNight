using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float sprintMultiplier = 1.6f;

    Animator animator;
    Vector2 inputVector;
    public Vector2 lastDirection = Vector2.down;

    Rigidbody2D rb;
    public bool isMoving;
    public bool isSprinting;
    public Vector2 InputDirection => inputDirection;

    static readonly int HorizontalHash = Animator.StringToHash("horizontal");
    static readonly int VerticalHash   = Animator.StringToHash("vertical");
    static readonly int LastHorizontalHash = Animator.StringToHash("lastHorizontal");
    static readonly int LastVerticalHash   = Animator.StringToHash("lastVertical");

    public static PlayerMovement FindInScene()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.GetComponent<PlayerMovement>() : null;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    Vector2 inputDirection;

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(x, y).normalized;
        isMoving = inputDirection != Vector2.zero;
        isSprinting = isMoving && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

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
        float currentSpeed = isSprinting ? speed * sprintMultiplier : speed;
        rb.MovePosition(rb.position + inputDirection * (currentSpeed * Time.fixedDeltaTime));
    }
}