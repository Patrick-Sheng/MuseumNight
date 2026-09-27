using UnityEngine;

public static class PlatformerModeController
{
    const float PlatformerGravityScale = 3f;

    public static void EnterPlatformerMode()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerMovement topDown = player.GetComponent<PlayerMovement>();
        if (topDown != null)
        {
            topDown.enabled = false;
            topDown.isMoving = false;
        }

        PlatformerPlayerController platformer = player.GetComponent<PlatformerPlayerController>();
        if (platformer == null) platformer = player.AddComponent<PlatformerPlayerController>();
        platformer.enabled = true;

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.gravityScale = PlatformerGravityScale;
            body.linearVelocity = Vector2.zero;
        }
    }

    public static void ExitPlatformerMode()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlatformerPlayerController platformer = player.GetComponent<PlatformerPlayerController>();
        if (platformer != null) platformer.enabled = false;

        PlayerMovement topDown = player.GetComponent<PlayerMovement>();
        if (topDown != null) topDown.enabled = true;

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
        }
    }
}
