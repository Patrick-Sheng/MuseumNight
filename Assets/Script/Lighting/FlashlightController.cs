using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] float angleOffset = -90f;
    [SerializeField] Vector2 positionOffset = Vector2.zero;

    void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();
    }

    void LateUpdate()
    {
        transform.localPosition = positionOffset;

        if (playerMovement == null) return;

        Vector2 dir = playerMovement.lastDirection;
        if (dir == Vector2.zero) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + angleOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
