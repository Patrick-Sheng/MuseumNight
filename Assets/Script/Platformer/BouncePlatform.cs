using UnityEngine;

public class BouncePlatform : MonoBehaviour
{
    [SerializeField] float bounceForce = 12f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        PlatformerPlayerController controller = collision.collider.GetComponent<PlatformerPlayerController>();
        if (controller != null && controller.enabled)
            controller.Bounce(bounceForce);
    }
}
