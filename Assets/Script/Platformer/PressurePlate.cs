using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] Mirror mirror;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && mirror != null)
            mirror.RotateStep();
    }
}
