using UnityEngine;

public class GroundCheckFollower : MonoBehaviour
{
    [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.6f, 0f);

    private Transform parent;

    private void Awake()
    {
        parent = transform.parent;
        if (parent == null)
        {
            Debug.LogError("GroundCheckFollower requires a parent (the player).");
        }
    }

    private void LateUpdate()
    {
        if (parent == null) return;

        // Keep GroundCheck locked to the player's bottom
        transform.position = parent.position + localOffset;
    }
}
