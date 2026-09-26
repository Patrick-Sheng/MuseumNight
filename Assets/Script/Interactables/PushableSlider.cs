using UnityEngine;

public class PushableSlider : MonoBehaviour
{
    [SerializeField] int rows = 1;
    [SerializeField] int columns = 3;
    [SerializeField] float gridSize = 1f;
    [SerializeField] Transform statueBody;
    // Which slot the statue's resting position (this transform) sits on. Only splits evenly
    // when columns is odd - keep this explicit instead of assuming a symmetric middle.
    [SerializeField] int homeIndex = 4;

    public float MinX => transform.position.x - homeIndex * gridSize;
    public float MaxX => transform.position.x + (columns - 1 - homeIndex) * gridSize;

    // Which of the `columns` grid slots statueBody currently sits on, 0-based from MinX.
    public int CurrentIndex
    {
        get
        {
            if (statueBody == null) return -1;
            float t = (statueBody.position.x - MinX) / gridSize;
            return Mathf.Clamp(Mathf.RoundToInt(t), 0, columns - 1);
        }
    }

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
