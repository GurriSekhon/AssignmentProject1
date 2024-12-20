using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    [SerializeField] private Image cardFrontImage;
    [SerializeField] private Image cardBackImage;
    [SerializeField] private Image cardMiddleImage;

    [SerializeField] private float flipSpeed = 5f; // Speed of the flip animation
    [SerializeField] private Button cardRevealbutton; //Button attached to card gameobject
    [HideInInspector] public Sprite cardFace;
    private bool isFlipped = false; // Indicates whether the card is currently flipped
    private bool isAnimating = false; // Prevents concurrent animations

    private void Awake()
    {
        cardRevealbutton.onClick.AddListener(OnCardClicked);
    }
    void Start()
    {
        // To Ensure card starts in the correct state (back of the card is visible, and front is hidden)
        ResetCardState();
    }

    public void SetCardFace(Sprite face)
    {
        cardFace = face;
        cardFrontImage.sprite = cardFace;
    }

    public void OnCardClicked()
    {
        if (isFlipped || isAnimating) return;

        StartCoroutine(Flip());
        GameManager.Instance.OnCardFlipped(this);
    }

    private IEnumerator Flip()
    {
        isAnimating = true;
        float rotation = 0f;

        // Step 1: Rotate halfway (90 degrees)
        while (rotation < 90f)
        {
            rotation += Time.deltaTime * flipSpeed * 180f;
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            yield return null;
        }

        // Step 2: Toggle Card's front image visibility to enable and back image to disabled at 90 degrees
        isFlipped = true;
        cardFrontImage.enabled = true;
        cardBackImage.enabled = false;

        // Step 3: Rotate card to full (180 degrees)
        while (rotation < 180f)
        {
            rotation += Time.deltaTime * flipSpeed * 180f;
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            yield return null;
        }

        transform.rotation = Quaternion.identity; // Reset rotation of the card
        isAnimating = false;
    }

    public void FlipBack()
    {
        StartCoroutine(FlipBackCoroutine());
    }

    private IEnumerator FlipBackCoroutine()
    {
        yield return new WaitForSeconds(1);
        isAnimating = true;
        float rotation = 180f;

        // Step 1: Rotate halfway back (90 degrees)
        while (rotation > 90f)
        {
            rotation -= Time.deltaTime * flipSpeed * 180f;
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            yield return null;
        }

        // Step 2: Swap visibility of the card again to card's back image
        isFlipped = false;
        cardFrontImage.enabled = false;
        cardBackImage.enabled = true;

        // Step 3: Rotate back to initial position (0 degrees)
        while (rotation > 0f)
        {
            rotation -= Time.deltaTime * flipSpeed * 180f;
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            yield return null;
        }

        transform.rotation = Quaternion.identity; // Reset rotation of the card once again 
        isAnimating = false;
    }

    public void Disappear()
    {
        StartCoroutine(FadeOutAndDisable());
    }

    private IEnumerator FadeOutAndDisable()
    {
        float cardScale = 1.2f;
        transform.localScale = Vector3.one * cardScale;
        yield return new WaitForSeconds(0.25f);
        while (cardScale > 1)
        {
            cardScale = cardScale - Time.deltaTime;
            transform.localScale = Vector3.one * cardScale;
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);
        CanvasGroup canvasGroup = gameObject.AddComponent<CanvasGroup>();
        float fadeDuration = 0.5f;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = 1 - (t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }

    public void ResetCardState()
    {
        // Ensure only the back image is visible initially
        cardFrontImage.enabled = false;
        cardBackImage.enabled = true;
        cardMiddleImage.enabled = true;
        isFlipped = false;
        transform.rotation = Quaternion.identity;
        isAnimating = false;
    }
}
