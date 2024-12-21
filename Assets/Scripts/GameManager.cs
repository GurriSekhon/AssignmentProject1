using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static Action<int, int> OnTilesMatch = null; //event to invoke in case two tiles gets matched
    public static Action<int> OnTurnFinished = null; //this is to invoke when player finishes the turn
    public static Action<float> OnTilesMisMatch = null; //event is invoked in case two tiles doesn't match
    public static Action OnLeveFinished = null; //this is to invoke when pairs found becomes equal to the required number of pairs
    public static Action OnCardFlip = null; //event gets invoked at each tile flip
    public static GameManager Instance;
    public GridLayoutGroup cardGrid;
    public GameObject cardPrefab;

    [Tooltip("Ensure that grid size is not an odd number")]
    [SerializeField] private int gridsize = 4;
    private int moves = 0;
    private int pairsFound = 0;
    private int totalPairs;
    private List<CardController> flippedCards = new List<CardController>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        SetupGame(gridsize, gridsize);
    }

    public void SetupGame(int rows, int cols)
    {
        if (gridsize % 2 != 0)
        {
            Debug.LogWarning("Grid size is not even. Please ensure you are not putting an odd number in grid size in Gamemanager", gameObject);
            return;
        }
        moves = 0;
        pairsFound = 0;
        OnTurnFinished?.Invoke(moves);
        OnTilesMatch?.Invoke(pairsFound, totalPairs);
        totalPairs = (rows * cols) / 2;

        // Generate card faces and shuffle them
        List<Sprite> cardFaces = GenerateCardFaces(totalPairs);
        List<Sprite> shuffledCards = ShuffleCards(cardFaces, rows * cols);

        // Clean up existing cards
        foreach (Transform child in cardGrid.transform)
            Destroy(child.gameObject);

        // Create new cards
        for (int i = 0; i < rows * cols; i++)
        {
            GameObject card = Instantiate(cardPrefab, cardGrid.transform);
            CardController cardController = card.GetComponent<CardController>();
            cardController.SetCardFace(shuffledCards[i]);
        }
    }

    public void OnCardFlipped(CardController card)
    {
        flippedCards.Add(card);
        OnCardFlip?.Invoke();

        if (flippedCards.Count == 2)
        {
            moves++;
            OnTurnFinished?.Invoke(moves);

            if (flippedCards[0].cardFace == flippedCards[1].cardFace)
            {
                Debug.Log("found a matching card pairs");
                pairsFound++;
                OnTilesMatch?.Invoke(pairsFound, totalPairs);
                if (pairsFound == totalPairs)
                    StartCoroutine(AnnounceLevelWin());
                foreach (var flippedcard in flippedCards)
                    flippedcard.Disappear();
            }
            else
            {
                foreach (var flippedcard in flippedCards)
                {
                    flippedcard.FlipBack();
                    flippedcard.PlayMismatchAnimation();
                }
            
                OnTilesMisMatch?.Invoke(0.5f);
            }
            flippedCards.Clear();
        }
    }

    private List<Sprite> GenerateCardFaces(int pairCount)
    {
        // Load all sprites from the spritesheet in the Resources folder
        Sprite[] allSprites = Resources.LoadAll<Sprite>("EmojiOne");

        // Ensure you have enough sprites for the required pairs. Log error in case if enough images are not present
        if (allSprites.Length < pairCount)
        {
            Debug.LogError("Not enough sprites in the spritesheet for the required pairs!");
            return new List<Sprite>();
        }

        List<Sprite> selectedSprites = new List<Sprite>();
        for (int i = 0; i < pairCount; i++)
        {
            string spriteName = "EmojiOne_" + i;
            Sprite matchingSprite = System.Array.Find(allSprites, sprite => sprite.name == spriteName);

            if (matchingSprite != null)
            {
                selectedSprites.Add(matchingSprite);
            }
            else
            {
                Debug.LogError("Sprite with name '" + spriteName + "' not found in the spritesheet!");
            }
        }
        return selectedSprites;
    }


    private List<Sprite> ShuffleCards(List<Sprite> cardFaces, int totalCards)
    {
        List<Sprite> shuffled = new List<Sprite>(cardFaces);
        shuffled.AddRange(cardFaces); // Duplicate for pairs
        for (int i = 0; i < shuffled.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(0, shuffled.Count);
            Sprite temp = shuffled[i];
            shuffled[i] = shuffled[randIndex];
            shuffled[randIndex] = temp;
        }
        return shuffled;
    }

    private IEnumerator AnnounceLevelWin()
    {
        yield return new WaitForSeconds(1f);
        OnLeveFinished?.Invoke();
    }
}