using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private TextMeshProUGUI turnsCounterText;
    [SerializeField] private TextMeshProUGUI matchCountText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject levelClearPanel;
    [SerializeField] private Button nextLevelButton;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        nextLevelButton.onClick.AddListener(ReloadScene);
    }


    private void OnEnable()
    {
        GameManager.OnGameStart += UpdateMatchesCountUI;
        GameManager.OnTilesMatch += UpdateMatchesCountUI;
        GameManager.OnTurnFinished += UpdateTurnsCountUI;
        GameManager.OnLeveFinished += DisplayLevelClearPanel;
        ScoreManager.OnScoreChanged += UpdateScoreUI;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= UpdateMatchesCountUI;
        GameManager.OnTilesMatch -= UpdateMatchesCountUI;
        GameManager.OnTurnFinished -= UpdateTurnsCountUI;
        GameManager.OnLeveFinished -= DisplayLevelClearPanel;
        ScoreManager.OnScoreChanged -= UpdateScoreUI;
    }


    private void UpdateTurnsCountUI(int turnsCount)
    {
        turnsCounterText.text = turnsCount.ToString();
    }

    private void UpdateMatchesCountUI(int pairsFound, int totalPairs)
    {
        matchCountText.text = pairsFound.ToString() + "/" + totalPairs;
    }
    
    private void UpdateScoreUI(int newScore)
    {
        scoreText.text = newScore.ToString();
    }

    private void DisplayLevelClearPanel()
    {
        levelClearPanel.SetActive(true);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}