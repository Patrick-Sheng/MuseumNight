using UnityEngine;

public class Mirror : MonoBehaviour
{
    [SerializeField] float rotationStep = 45f;

    public void RotateStep()
    {
        transform.Rotate(0f, 0f, rotationStep);
        Debug.Log($"{name} rotated to {transform.eulerAngles.z} degrees", this);
    }
}
