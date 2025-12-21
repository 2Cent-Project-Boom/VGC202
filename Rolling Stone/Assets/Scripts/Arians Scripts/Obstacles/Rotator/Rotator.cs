using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Degrees per second of rotation.")]
    [SerializeField] private float rotationSpeed = 90f;

    [Tooltip("Axis of rotation in local space (e.g., (0,1,0) = rotate around Y).")]
    [SerializeField] private Vector3 localAxis = Vector3.up;

    [Tooltip("Randomize starting rotation so multiple instances look varied.")]
    [SerializeField] private bool randomizeStartRotation = true;

    private void OnEnable()
    {
        if (randomizeStartRotation)
        {
            float randomAngle = Random.Range(0f, 360f);
            transform.Rotate(localAxis.normalized * randomAngle, Space.Self);
        }
    }

    private void Update()
    {
        if (rotationSpeed == 0f || localAxis == Vector3.zero)
            return;

        //Rotate every frame
        float deltaAngle = rotationSpeed * Time.deltaTime;
        transform.Rotate(localAxis.normalized * deltaAngle, Space.Self);
    }
}
