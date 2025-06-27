using UnityEngine;



public class PauseManager : MonoBehaviour
{
    public GameObject pauseScreen; 
    public float pauseTime = 30f; 
    public GameObject mainCamera;

    private bool isPaused = false;

    void Start()
    {
        if (pauseScreen != null)
            pauseScreen.SetActive(false);

        Invoke(nameof(PauseGame), pauseTime);
    }

    void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;

            if (pauseScreen != null)
            {
                pauseScreen.SetActive(true);
                PositionPauseScreen();
            }
        }
    }

    public void OnPauseScreenClicked()
    {
        if (isPaused)
        {
            InternalResumeGame();
        }
    }

    private void InternalResumeGame()
    {
        isPaused = false;

        if (pauseScreen != null)
            pauseScreen.SetActive(false);

        Invoke(nameof(PauseGame), 240f);
    }

    private void PositionPauseScreen()
    {
        Transform cameraTransform = mainCamera.transform;
        pauseScreen.transform.position = cameraTransform.position + cameraTransform.forward * 1.3f;
        pauseScreen.transform.rotation = Quaternion.LookRotation(cameraTransform.forward);
    }
}

