using UnityEngine;

public class BeamActivatorPlate : MonoBehaviour
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Vector2 initialDirection = Vector2.down;

    GameObject activeProjectile;
    bool playerInside;

    // OnTriggerStay2D as a fallback in case the player is already standing on
    // the plate when the scene loads, which would never fire OnTriggerEnter2D.
    void OnTriggerEnter2D(Collider2D other) => TryActivate(other);
    void OnTriggerStay2D(Collider2D other) => TryActivate(other);

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    void TryActivate(Collider2D other)
    {
        if (playerInside || !other.CompareTag("Player") || projectilePrefab == null || spawnPoint == null) return;
        playerInside = true;

        if (activeProjectile != null)
            Destroy(activeProjectile);

        activeProjectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        activeProjectile.GetComponent<LightProjectile>().Init(initialDirection);
        GameAudio.Play(GameAudio.Cue.Room3BallLaunch);
        Debug.Log($"Sun activated by {name}: spawned projectile at {spawnPoint.position} heading {initialDirection}", this);
    }
}
