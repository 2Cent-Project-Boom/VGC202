using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text scoreText;

    [Header("Formatting")]
    [SerializeField] private float metersPerUnit = 1f;

    private void Awake()
    {
        if (scoreText == null)
            scoreText = GetComponent<TMP_Text>();

        if (player == null)
            player = FindPlayerTransform();

        if (scoreText == null)
        {
            Debug.LogError("[Score] scoreText is not assigned and could not be found.", this);
            enabled = false;
            return;
        }

        if (player == null)
        {
            Debug.LogError("[Score] player is not assigned and could not be found (tag: Player).", this);
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        float meters = player.position.z * metersPerUnit;
        scoreText.text = meters.ToString("0");
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
            return null; // Tag doesn't exist
        }

        if (players == null || players.Length == 0) return null;
        if (players.Length == 1) return players[0].transform;

        // If multiple, prefer the one that actually has a BallMotor (real playable object)
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].GetComponent<BallMotor>() != null)
                return players[i].transform;
        }

        return players[0].transform;
    }
}
