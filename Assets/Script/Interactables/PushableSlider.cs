using UnityEngine;

public class PushableSlider : MonoBehaviour
{
    [SerializeField] int rows = 1;
    [SerializeField] int columns = 3;
    [SerializeField] float gridSize = 1f;

    public float MinX => transform.position.x - (columns - 1) * 0.5f * gridSize;
    public float MaxX => transform.position.x + (columns - 1) * 0.5f * gridSize;

    public bool Contains(float x)
    {
        const float tolerance = 0.01f;
        return x >= MinX - tolerance && x <= MaxX + tolerance;
    }

    void OnDrawGizmos()
    {
        Vector3 size = new Vector3(columns * gridSize, rows * gridSize, 0.1f);
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawCube(transform.position, size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
