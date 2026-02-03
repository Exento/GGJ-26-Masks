using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int[] _scores = new int[4];

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public int GetScore(int playerIndex) => _scores[playerIndex];

    public void AddScore(int playerIndex, int amount)
    {
        _scores[playerIndex] += amount;
        Debug.Log($"Player {playerIndex} score: {_scores[playerIndex]}");
    }

    public void ResetAll()
    {
        for (int i = 0; i < _scores.Length; i++) _scores[i] = 0;
    }
}
