using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PushableObject : MonoBehaviour
{
    [SerializeField] float pushSpeed = 4f;
    [SerializeField] float gridSize = 1f;
    [SerializeField] float pushAlignmentThreshold = 0.5f;

    Rigidbody2D rb;
    BoxCollider2D boxCollider;
    bool isMoving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isMoving || !collision.gameObject.CompareTag("Player"))
            return;

        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player == null || !player.isMoving)
            return;

        Vector2 pushDirection = GetCardinalDirection((Vector2)transform.position - (Vector2)collision.transform.position);

        if (Vector2.Dot(pushDirection, player.InputDirection) < pushAlignmentThreshold)
            return;

        Vector2 destination = rb.position + pushDirection * gridSize;
        if (IsBlocked(destination))
            return;

        StartCoroutine(MoveTo(destination));
    }

    static Vector2 GetCardinalDirection(Vector2 direction)
    {
        return Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
            ? new Vector2(Mathf.Sign(direction.x), 0f)
            : new Vector2(0f, Mathf.Sign(direction.y));
    }

    bool IsBlocked(Vector2 destination)
    {
        Vector2 size = Vector2.Scale(boxCollider.size, transform.lossyScale);
        Collider2D[] hits = Physics2D.OverlapBoxAll(destination, size, 0f);
        foreach (Collider2D hit in hits)
        {
            if (hit == boxCollider || hit.isTrigger || hit.CompareTag("Player"))
                continue;
            return true;
        }
        return false;
    }

    IEnumerator MoveTo(Vector2 destination)
    {
        isMoving = true;
        while ((destination - rb.position).sqrMagnitude > 0.0001f)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, destination, pushSpeed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }
        rb.MovePosition(destination);
        isMoving = false;
    }
}
