using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip correctHitSound;
    public AudioClip wrongHitSound;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject); // optional if you want persistence across scenes

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    public void PlayCorrectHitSound()
    {
        if (correctHitSound != null)
            audioSource.PlayOneShot(correctHitSound);
    }

    public void PlayWrongHitSound()
    {
        if (wrongHitSound != null)
            audioSource.PlayOneShot(wrongHitSound);
    }
}
