using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GridLayoutGroup cardGrid;
    public GameObject cardPrefab;

    [Tooltip("Ensure that grid size is not an odd number")]
    [SerializeField] private int gridsize = 4;
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
        if (flippedCards.Count == 2)
        {
            if (flippedCards[0].cardFace == flippedCards[1].cardFace)
            {
                Debug.Log("found a matching card pairs");
                foreach (var flippedcard in flippedCards)
                    Destroy(flippedcard.gameObject, 1);
            }
            else
            {
                foreach (var flippedcard in flippedCards)
                    flippedcard.FlipBack();
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
            int randIndex = Random.Range(0, shuffled.Count);
            Sprite temp = shuffled[i];
            shuffled[i] = shuffled[randIndex];
            shuffled[randIndex] = temp;
        }
        return shuffled;
    }
}