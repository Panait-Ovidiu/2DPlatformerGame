using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSinglePlayer: MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void ShowWin()
    {
        winPanel.SetActive(true);
    }

    public void MainMenu(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void NextLevelScene(string sceneToLoad)
    {
        int groundTileIndex = Random.Range(0, 8);
        PlayerPrefs.SetInt("groundTileIndex", groundTileIndex);
        float seed = Random.Range(0, 100000);
        PlayerPrefs.SetFloat("seed", seed);
        int enemiesNr = Random.Range(0, 5);
        PlayerPrefs.SetInt("enemiesNr", enemiesNr);
        SceneManager.LoadScene(sceneToLoad);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
