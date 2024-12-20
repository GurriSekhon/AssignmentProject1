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

        while (rotation < 90f)
        {
            rotation += Time.deltaTime * flipSpeed * 180f;
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            yield return null;
        }

        isFlipped = true;
        cardFrontImage.enabled = true;
        cardBackImage.enabled = false;

        while (rotation < 180f)
        {
            rotation += Time.deltaTime * flipSpeed * 180f;
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            yield return null;
        }

        transform.rotation = Quaternion.identity; // Reset rotation
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

        while (rotation > 90f)
        {
            rotation -= Time.deltaTime * flipSpeed * 180f;
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            yield return null;
        }

        isFlipped = false;
        cardFrontImage.enabled = false;
        cardBackImage.enabled = true;

        while (rotation > 0f)
        {
            rotation -= Time.deltaTime * flipSpeed * 180f;
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            yield return null;
        }

        transform.rotation = Quaternion.identity; // Reset rotation
        isAnimating = false;
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
