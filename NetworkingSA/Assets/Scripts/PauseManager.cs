using UnityEngine;
using TMPro;
using MixedReality.Toolkit.UX;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseScreen; // Reference to the pause Canvas
    public GameObject playButton; // Reference to the MRTK3 Play button
    public float pauseTime = 30f; // Time in seconds (e.g., 7 minutes = 420 seconds)
    public GameObject mainCamera;

    private bool isPaused = false;

    void Start()
    {
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false); // Ensure the pause screen is hidden initially
             playButton.SetActive(false);
        }

        if (playButton != null)
        {
            //var interactable = playButton;
            var interactable = playButton.AddComponent<StatefulInteractable>();
            interactable.selectMode = InteractableSelectMode.Single;
            interactable.OnClicked.AddListener(ResumeGame); // Add listener to the Play button
        }

        // Start the timer for pausing
        Invoke(nameof(PauseGame), pauseTime);
    }

    void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0f; // Pause the game (freeze time)

            if (pauseScreen != null)
            {
                pauseScreen.SetActive(true); // Show the pause screen
                playButton.SetActive(true);
                PositionPauseScreen();
            }
        }
    }

    public void ResumeGame()
    {
          Debug.Log("Quad clicked!");
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1f; // Resume the game (unfreeze time)

            if (pauseScreen != null)
            {
                pauseScreen.SetActive(false); // Hide the pause screen
                playButton.SetActive(false);
            }

            // Optionally, reset the timer if needed
            Invoke(nameof(PauseGame), pauseTime);
        }
    }

    private void PositionPauseScreen()
    {
        // Position the pause screen dynamically in front of the user
        // Transform cameraTransform = mainCamera.transform;
        // pauseScreen.transform.position = cameraTransform.position + cameraTransform.forward * 1f;
        // pauseScreen.transform.rotation = Quaternion.LookRotation(cameraTransform.forward);
    }
}
