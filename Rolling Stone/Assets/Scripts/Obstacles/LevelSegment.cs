using UnityEngine;

public class LevelSegment : MonoBehaviour
{
    [Header("Segment Settings")]
    [Tooltip("How long this segment is along the Z axis.")]
    public float length = 25f;

    [Tooltip("Where the previous segment should connect.")]
    public Transform entryPoint;

    [Tooltip("Where the next segment should connect.")]
    public Transform exitPoint;

    private void OnValidate()
    {
        // If entry/exit are not assigned, try to auto-assign children named that way
        if (entryPoint == null)
        {
            Transform child = transform.Find("EntryPoint");
            if (child != null) entryPoint = child;
        }

        if (exitPoint == null)
        {
            Transform child = transform.Find("ExitPoint");
            if (child != null) exitPoint = child;
        }
    }

    private void OnDrawGizmos()
    {
        if (entryPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(entryPoint.position, 0.2f);
        }

        if (exitPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(exitPoint.position, 0.2f);
        }

        if (entryPoint != null && exitPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(entryPoint.position, exitPoint.position);
        }
    }
}
