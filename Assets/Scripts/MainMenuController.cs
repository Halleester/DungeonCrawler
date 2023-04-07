using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject lanternNormal;
    public GameObject lanternWin;
    public GameObject lanternLose;

    public string gameScene;
    public GameObject creditsScreen;
    public GameObject menuScreen;
    private bool onCredits = false;

    public void ActionPlayGame()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void ActionToggleCredits() {
        onCredits = !onCredits;
        creditsScreen.SetActive(onCredits);
        menuScreen.SetActive(!onCredits);
    }

    public void ActionQuitGame()
    {
        Application.Quit();
    }
}
