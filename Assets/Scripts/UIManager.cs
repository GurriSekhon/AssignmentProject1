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
    [SerializeField] private TextMeshProUGUI comboText; //Text gameobject to be displayed for combo
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
        GameManager.OnGameStart += InitializeUI;
        GameManager.OnTilesMatch += UpdateMatchesCountUI;
        GameManager.OnTurnFinished += UpdateTurnsCountUI;
        GameManager.OnLeveFinished += DisplayLevelClearPanel;
        ScoreManager.OnScoreChanged += UpdateScoreUI;
        ScoreManager.OnComboHit += ShowComboCount;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= InitializeUI;
        GameManager.OnTilesMatch -= UpdateMatchesCountUI;
        GameManager.OnTurnFinished -= UpdateTurnsCountUI;
        GameManager.OnLeveFinished -= DisplayLevelClearPanel;
        ScoreManager.OnScoreChanged -= UpdateScoreUI;
        ScoreManager.OnComboHit -= ShowComboCount;
    }

    private void UpdateTurnsCountUI(int turnsCount)
    {
        turnsCounterText.text = turnsCount.ToString();
    }

    private void UpdateMatchesCountUI(int pairsFound, int totalPairs)
    {
        matchCountText.text = pairsFound.ToString() + "/" + totalPairs;
    }
    private void InitializeUI(int pairsFound, int totalPairs, int moves)
    {
        matchCountText.text = pairsFound.ToString() + "/" + totalPairs;
        turnsCounterText.text = moves.ToString();
    }

    private void UpdateScoreUI(int newScore)
    {
        scoreText.text = newScore.ToString();
    }

    private void DisplayLevelClearPanel()
    {
        levelClearPanel.SetActive(true);
    }

    private void ShowComboCount(int comboCount)
    {
        comboText.StopAllCoroutines();
        StartCoroutine(ShowComboText(comboCount));
    }

    private IEnumerator ShowComboText(int count)
    {
        comboText.gameObject.SetActive(true);
        comboText.text = "Combo X" + count;
        Color initialColor = comboText.color;
        initialColor.a = 1;
        comboText.color = initialColor;
        yield return new WaitForSeconds(0.075f);
        comboText.transform.localRotation = Quaternion.Euler(0, 0, -7);
        yield return new WaitForSeconds(0.075f);
        comboText.transform.localRotation = Quaternion.Euler(0, 0, 7);
        yield return new WaitForSeconds(0.075f);
        comboText.transform.localRotation = Quaternion.identity;
        yield return new WaitForSeconds(0.25f);
        while (comboText.color.a > 0)
        {
            yield return null;
            initialColor.a = comboText.color.a - 0.01f;
            comboText.color = initialColor;
        }
        comboText.gameObject.SetActive(false);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}