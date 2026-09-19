using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PushableObject : MonoBehaviour
{
    [SerializeField] float pushSpeed = 4f;
    [SerializeField] float gridSize = 1f;
    [SerializeField] float pushAlignmentThreshold = 0.5f;
    [SerializeField] PushableSlider slider;

    Rigidbody2D rb;
    BoxCollider2D boxCollider;
    bool isMoving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (slider == null)
            slider = GetComponentInChildren<PushableSlider>();
        if (slider == null && transform.parent != null)
            slider = transform.parent.GetComponentInChildren<PushableSlider>();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isMoving || !collision.gameObject.CompareTag("Player"))
            return;

        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player == null || !player.isMoving)
            return;

        Vector2 relative = (Vector2)transform.position - (Vector2)collision.transform.position;
        if (Mathf.Abs(relative.x) <= Mathf.Abs(relative.y))
            return; // statues can only be pushed horizontally

        Vector2 pushDirection = new Vector2(Mathf.Sign(relative.x), 0f);

        if (Vector2.Dot(pushDirection, player.InputDirection) < pushAlignmentThreshold)
            return;

        Vector2 destination = rb.position + pushDirection * gridSize;
        if (IsBlocked(destination) || (slider != null && !slider.Contains(destination.x)))
            return;

        StartCoroutine(MoveTo(destination));
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
