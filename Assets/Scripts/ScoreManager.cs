using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    // Event gets invoked whenever the score changes.
    public static event Action<int> OnScoreChanged;

    [Header("Scoring Rules")]
    [Tooltip("Points awarded for each matched pair.")]
    [SerializeField] private int matchPoints = 1;

    [Tooltip("Extra points added for each consecutive match in a combo.")]
    [SerializeField] private int comboBonus = 2;

    private int currentScore = 0;
    private int comboCount = 0; // Tracks consecutive matches

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        // Subscribe to relevant game events
        GameManager.OnTilesMatch += HandleTilesMatch;
        GameManager.OnTilesMisMatch += HandleTilesMismatch;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        GameManager.OnTilesMatch -= HandleTilesMatch;
        GameManager.OnTilesMisMatch -= HandleTilesMismatch;
    }

    // Reset score and combo count when starting a new level.
    public void ResetScore()
    {
        currentScore = 0;
        comboCount = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    private void HandleTilesMatch(int pairsFound, int totalPairs)
    {
        // Increase combo count
        comboCount++;

        // Here is how scoring works:
        // (matchPoints) + (comboBonus * (comboCount - 1))

        // For example, if comboCount = 1 (first match), no combo bonus will be given yet.

        // If comboCount = 2, then there's 1 "consecutive match" bonus, and so on for more consecutive matches
        int pointsForThisMatch = matchPoints + comboBonus * (comboCount - 1);

        if (comboCount >= 2)
            Debug.Log("Combo: " + comboCount);
        currentScore += pointsForThisMatch;
        OnScoreChanged?.Invoke(currentScore);
    }

    private void HandleTilesMismatch(float delay)
    {
        // A mismatch breaks the combo and resets it to 0
        comboCount = 0;
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}
