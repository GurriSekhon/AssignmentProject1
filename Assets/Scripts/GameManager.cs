using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static Action<int, int, int> OnGameStart = null; //event to invoke when game starts
    public static Action<int, int> OnTilesMatch = null; //event to invoke in case two tiles gets matched
    public static Action<int> OnTurnFinished = null; //this is to invoke when player finishes the turn
    public static Action<float> OnTilesMisMatch = null; //event is invoked in case two tiles doesn't match
    public static Action OnLeveFinished = null; //this is to invoke when pairs found becomes equal to the required number of pairs
    public static Action OnCardFlip = null; //event gets invoked at each tile flip
    public static Action OnDataSaveCalled = null; //event gets invoked when call for data saving is made
    public static GameManager Instance;

    public RectTransform cardContainer; // The container we attached ManualLayoutManager to
    private ManualLayoutManager layoutManager; // reference to the script
    public GameObject cardPrefab;
    [Tooltip("Seed for shuffling. If 0, a random seed will be generated.")]
    [SerializeField] private int shuffleSeed = 0;

    [Tooltip("Ensure that grid size is not an odd number")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int cols = 4;

    [SerializeField] private float initialCardsHideDelay = 1.5f;

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
        layoutManager = cardContainer.GetComponent<ManualLayoutManager>();
    }

    void Start()
    {
        // Check if there's a saved game in progress
        if (PlayerPrefs.HasKey("GameInProgress"))
        {
            Debug.Log("Saved game found. Looking data..");
            LoadGame();
        }
        else
        {
            Debug.Log("No saved game found! Starting from scratch");
            SetupGame(rows, cols);
        }
    }

    public void SetupGame(int rows, int cols)
    {
        if ((rows * cols) % 2 != 0)
        {
            Debug.LogWarning("Grid size must be even for pairs to match!", gameObject);
            return;
        }

        //If no seed is specified, pick a random one
        if (shuffleSeed == 0)
        {
            shuffleSeed = UnityEngine.Random.Range(1, 999999);
            Debug.Log($"No seed specified; using random seed: {shuffleSeed}");
        }

        // Set the random state for a deterministic shuffle
        UnityEngine.Random.InitState(shuffleSeed);

        totalPairs = (rows * cols) / 2;
        OnGameStart?.Invoke(pairsFound, totalPairs, moves);

        // Generate card faces and shuffle them
        List<Sprite> cardFaces = GenerateCardFaces(totalPairs);
        List<Sprite> shuffledCards = ShuffleCards(cardFaces, rows * cols);

        // Clean up existing cards
        foreach (Transform child in cardContainer.transform)
            Destroy(child.gameObject);

        List<CardController> cardControllers = new List<CardController>();

        // Create new cards
        for (int i = 0; i < rows * cols; i++)
        {
            GameObject card = Instantiate(cardPrefab);
            CardController controller = card.GetComponent<CardController>();
            controller.SetCardFace(shuffledCards[i]);
            cardControllers.Add(controller);
        }

        // Convert to List<GameObject> for the layout script
        List<GameObject> cardObjects = new List<GameObject>();
        foreach (var ctrl in cardControllers)
            cardObjects.Add(ctrl.gameObject);

        // Now call the manual layout arrangement
        layoutManager.ArrangeCards(rows, cols, cardObjects);

        StartCoroutine(RevealAndHideCards(cardControllers));
    }



    private IEnumerator RevealAndHideCards(List<CardController> cardControllers)
    {
        yield return null;

        //When game starts all cards gets revealed temporarily to the player without any animations
        foreach (var card in cardControllers)
        {
            card.RevealAllCardsToPlayer();
        }

        // Allow players to memorize cards for reveal duration
        yield return new WaitForSeconds(initialCardsHideDelay);

        // Hide the cards again
        foreach (var card in cardControllers)
        {
            card.FlipBack();
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
                //Debug.Log("found a matching card pairs");
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

    public void SaveGame()
    {
        // Save basic data (seed, grid size, moves, pairs found)
        PlayerPrefs.SetInt("Seed", shuffleSeed);
        PlayerPrefs.SetInt("Rows", rows);
        PlayerPrefs.SetInt("Cols", cols);
        PlayerPrefs.SetInt("Moves", moves);
        PlayerPrefs.SetInt("PairsFound", pairsFound);

        // Mark that a game is in progress
        PlayerPrefs.SetInt("GameInProgress", 1);

        PlayerPrefs.Save();
        Debug.Log("Game saved via PlayerPrefs!");
    }

    public void LoadGame()
    {
        
        // Retrieve basic data
        shuffleSeed = PlayerPrefs.GetInt("Seed", 0);
        rows = PlayerPrefs.GetInt("Rows", 4);
        cols = PlayerPrefs.GetInt("Cols", 4);
        moves = PlayerPrefs.GetInt("Moves", 0);
        pairsFound = PlayerPrefs.GetInt("PairsFound", 0);

        // Re-setup the game with the stored seed, rows, cols
        SetupGame(rows, cols);

        Debug.Log("Game loaded from PlayerPrefs!");
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            OnDataSaveCalled?.Invoke();
            SaveGame();
        }
    }
}
