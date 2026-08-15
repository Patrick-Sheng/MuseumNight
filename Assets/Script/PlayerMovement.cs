using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed = 5f;

    Rigidbody2D rb;
    public bool isMoving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    Vector2 inputDirection;

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(x, y).normalized;
        isMoving = inputDirection != Vector2.zero;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + inputDirection * (speed * Time.fixedDeltaTime));
    }
}