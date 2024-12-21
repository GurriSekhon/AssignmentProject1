using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip flipSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip mismatchSound;
    [SerializeField] private AudioClip levelCompleteSound;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        GameManager.OnTilesMatch += HandleTilesMatch;
        GameManager.OnLeveFinished += HandleLevelFinished;
        GameManager.OnCardFlip += HandleCardFlip;
        GameManager.OnTilesMisMatch += HandleTilesMismatch;
    }

    private void OnDisable()
    {
        GameManager.OnTilesMatch -= HandleTilesMatch;
        GameManager.OnLeveFinished -= HandleLevelFinished;
        GameManager.OnCardFlip -= HandleCardFlip;
        GameManager.OnTilesMisMatch -= HandleTilesMismatch;
    }

    private void HandleCardFlip()
    {
        PlaySound(flipSound);
    }

    private void HandleTilesMatch(int pairsFound, int totalPairs)
    {
        PlaySound(matchSound);
    }

    private void HandleTilesMismatch(float sfxDelay)
    {
        StartCoroutine(PlayTilesMismatchSfxDelayed(sfxDelay));
    }

    IEnumerator PlayTilesMismatchSfxDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySound(mismatchSound);
    }

    private void HandleLevelFinished()
    {
        PlaySound(levelCompleteSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
