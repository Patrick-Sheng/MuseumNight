using UnityEngine;

/// Swaps the player's Animator to the torch sprite set for as long as this room is
/// loaded, reverting when it unloads.
public class TorchAnimationZone : MonoBehaviour
{
    [SerializeField] RuntimeAnimatorController normalController;
    [SerializeField] RuntimeAnimatorController torchController;

    void OnEnable()
    {
        Animator animator = FindPlayerAnimator();
        if (animator != null && torchController != null)
            animator.runtimeAnimatorController = torchController;
    }

    void OnDisable()
    {
        Animator animator = FindPlayerAnimator();
        if (animator != null && normalController != null)
            animator.runtimeAnimatorController = normalController;
    }

    static Animator FindPlayerAnimator()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.GetComponent<Animator>() : null;
    }
}
