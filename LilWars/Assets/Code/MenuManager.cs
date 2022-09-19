using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public UnityEngine.UI.Text diffText;

    //private float difficulty = 0.8f;

    private void Start()
    {
        diffText.text = "Difficulty: " + GameManager.instance.difficulty;
    }

    public void GoToLevel(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void GoToLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void QuitGame()
    {
        Debug.Log("Exiting game");
        Application.Quit();
    }

    public void RaiseDifficulty()
    {
        GameManager.instance.difficulty += 0.1f;
        diffText.text = "Difficulty: " + GameManager.instance.difficulty;
    }

    public void LowerDifficulty()
    {
        GameManager.instance.difficulty -= 0.1f;
        diffText.text = "Difficulty: " + GameManager.instance.difficulty;
    }
}
