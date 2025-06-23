using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    private void Start()
    {
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ResetGame()
    {
        Time.timeScale = 1f;
        TransitionToLevel("MainMenu");
    }
    
    private void TransitionToLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("GameManager: Attempted to transition to an empty scene name.");
            return;
        }
        Debug.Log($"GameManager: Transitioning to scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
