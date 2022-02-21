using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] MenuCharacters singlePlayerChar;
    [SerializeField] List<MenuCharacters> multiPlayerChar;

    [SerializeField] private Text cherriesText;
    private int cherries = 0;

    bool isSinglePlayerHover;
    bool isMultiPlayerHover;

    private void Start()
    {
        cherries = PlayerPrefs.GetInt("cherries");
        cherriesText.text = "" + cherries;
    }

    private void Update()
    {
        if(isSinglePlayerHover)
        {
            singlePlayerChar.Jump();
        }

        if(isMultiPlayerHover)
        {
            foreach(MenuCharacters character in multiPlayerChar) {
                character.Jump();
            }
        }
    }

    public void SinglePlayerScene(string sceneToLoad)
    {
        int groundTileIndex = Random.Range(0, 8);
        PlayerPrefs.SetInt("groundTileIndex", groundTileIndex);
        float seed = Random.Range(0, 100000);
        PlayerPrefs.SetFloat("seed", seed);
        int enemiesNr = Random.Range(0, 5);
        PlayerPrefs.SetInt("enemiesNr", enemiesNr);
        SceneManager.LoadScene(sceneToLoad);
    }

    public void MultiplayerScene(string sceneToLoad)
    {
        PlayerPrefs.SetInt("roundsPlayerOne", 0);
        PlayerPrefs.SetInt("roundsPlayerTwo", 0);
        PlayerPrefs.SetInt("totalRounds", 1);
        SceneManager.LoadScene(sceneToLoad);

    }

    public void SinglePlayerHover()
    {
        isSinglePlayerHover = true;
    }

    public void SinglePlayerExitHover()
    {
        isSinglePlayerHover = false;
    }

    public void MultiPlayerHover()
    {
        isMultiPlayerHover = true;
    }

    public void MultiPlayerExitHover()
    {
        isMultiPlayerHover = false;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
