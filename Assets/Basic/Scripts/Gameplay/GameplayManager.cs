using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    #region Parameters

    [SerializeField]
    private InputManager _inputManager;

    [SerializeField]
    private GameObject _pauseMenuCanvas;

    [SerializeField]
    private string _gameSceneName;

    #endregion


    #region Main Functions

    private void Start()
    {
        _inputManager.OnPauseMenuInput += Pause;
        _inputManager.OnResumeGameInput += ResumeGame;
    }

    private void OnDestroy()
    {
        _inputManager.OnPauseMenuInput -= Pause;
        _inputManager.OnResumeGameInput -= ResumeGame;
    }
    #endregion


    #region Gameplay Menu & Button Functions

    public void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _pauseMenuCanvas.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        _pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void BackToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(_gameSceneName);
    }
    #endregion
}
