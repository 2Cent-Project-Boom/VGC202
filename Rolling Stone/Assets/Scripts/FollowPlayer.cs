using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -10f);

    [Header("Smoothing")]
    [SerializeField, Range(0f, 30f)] private float followSharpness = 12f;

    private void Awake()
    {
        if (player == null)
            player = FindPlayerTransform();
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 target = player.position + offset;

        if (followSharpness <= 0f)
        {
            transform.position = target;
            return;
        }

        float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, target, t);
    }

    private static Transform FindPlayerTransform()
    {
        GameObject[] players;
        try
        {
            players = GameObject.FindGameObjectsWithTag("Player");
        }
        catch
        {
            return null;
        }

        if (players == null || players.Length == 0) return null;
        if (players.Length == 1) return players[0].transform;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].GetComponent<BallMotor>() != null)
                return players[i].transform;
        }

        return players[0].transform;
    }
}
