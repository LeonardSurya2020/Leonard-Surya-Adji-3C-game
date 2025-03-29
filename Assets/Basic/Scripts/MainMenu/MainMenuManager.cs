using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    #region Parameters
    [Header("Main Menu Canvas")]
    [SerializeField]
    private GameObject _mainMenuCanvas;
    [SerializeField]
    private GameObject _creditMenuCanvas;

    [Header("Scene Name")]
    [SerializeField]
    private string _gameSceneName;
    #endregion

    #region Main Menu & button Functions
    public void Play()
    {
        SceneManager.LoadScene(_gameSceneName);
    }

    public void Exit()
    {
        Debug.Log("Exit");
        Application.Quit();
    }

    public void CreditMenuCanvas()
    {
        if(_mainMenuCanvas != null && _creditMenuCanvas != null)
        {
            _creditMenuCanvas.SetActive(true);
            _mainMenuCanvas.SetActive(false);
        }
    }

    public void BackToMainMenuCanvasMenu()
    {
        if (_mainMenuCanvas != null && _creditMenuCanvas != null)
        {
            _creditMenuCanvas.SetActive(false);
            _mainMenuCanvas.SetActive(true);
        }
    }
    #endregion
}
