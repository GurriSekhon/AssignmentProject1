using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    public Image cardFrontImage; 
    public Image cardBackImage; 
    public Image cardMiddleImage;

    private bool isFlipped = false;

    void Start()
    {
        // To Ensure card starts in the correct state (back of the card is visible, and front is hidden)
        ResetCardState();
    }

    public void SetCardFace(Sprite face)
    {
        cardFrontImage.sprite = face;
    }

    public void ResetCardState()
    {
        // Ensure only the back image is visible initially
        cardFrontImage.enabled = false;
        cardBackImage.enabled = true;
        cardMiddleImage.enabled = true;
        isFlipped = false;
        transform.rotation = Quaternion.identity;
    }
}
