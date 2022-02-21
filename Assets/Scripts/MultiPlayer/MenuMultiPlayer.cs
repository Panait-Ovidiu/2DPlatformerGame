using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuMultiPlayer : MonoBehaviour
{
    [SerializeField] private GameObject roundPanel;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private float roundPanelShowTime = 3f;
    [SerializeField] private float roundEndPanelShowTime = 3f;

    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private int roundsNeedForWin = 3;

    [SerializeField] Transform projectiles;

    [SerializeField] private PlayerOne playerOne;
    [SerializeField] private List<GameObject> playerOneCherries;
    private int roundsPlayerOne = 0;

    [SerializeField] private PlayerTwo playerTwo;
    [SerializeField] private List<GameObject> playerTwoCherries;
    private int roundsPlayerTwo = 0;

    private int totalRounds = 0;
    private bool roundEnded;

    // Start is called before the first frame update
    void Start()
    {
        roundPanel.SetActive(true);

        totalRounds = PlayerPrefs.GetInt("totalRounds");
        roundText.text = "Round " + totalRounds;
        Invoke("HideRoundsPanel", roundPanelShowTime);

        roundsPlayerOne = PlayerPrefs.GetInt("roundsPlayerOne");
        if (roundsPlayerOne > 0)
        {
            for (int i = 0; i < roundsPlayerOne; i++)
            {
                playerOneCherries[i].SetActive(true);
            }
        }

        roundsPlayerTwo = PlayerPrefs.GetInt("roundsPlayerTwo");
        if (roundsPlayerTwo > 0)
        {
            for (int i = 0; i < roundsPlayerTwo; i++)
            {
                playerTwoCherries[i].SetActive(true);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!roundEnded)
        {
            int projectilesNr = projectiles.childCount;

            if (playerOne.GetIsDead() && !playerTwo.GetIsDead() && (projectilesNr == 0)) // Player Two Wins
            {
                roundEnded = true;
                totalRounds++;
                PlayerPrefs.SetInt("totalRounds", totalRounds);

                playerTwoCherries[roundsPlayerTwo].SetActive(true);
                roundsPlayerTwo++;
                PlayerPrefs.SetInt("roundsPlayerTwo", roundsPlayerTwo);

                if (roundsPlayerTwo == roundsNeedForWin)
                {
                    winPanel.SetActive(true);
                    winText.text = "Ice Wizard Wins Match";
                }
                else
                {
                    roundPanel.SetActive(true);
                    roundText.text = "Ice Wizard Wins";

                    Invoke("NextLevel", roundEndPanelShowTime);
                }
            }
            else if (!playerOne.GetIsDead() && playerTwo.GetIsDead() && (projectilesNr == 0)) // Player One Wins
            {
                roundEnded = true;
                totalRounds++;
                PlayerPrefs.SetInt("totalRounds", totalRounds);

                playerOneCherries[roundsPlayerOne].SetActive(true);
                roundsPlayerOne++;
                PlayerPrefs.SetInt("roundsPlayerOne", roundsPlayerOne);

                if (roundsPlayerOne == roundsNeedForWin)
                {
                    winPanel.SetActive(true);
                    winText.text = "Archer Wins Match";
                }
                else
                {
                    roundPanel.SetActive(true);
                    roundText.text = "Archer Wins";

                    Invoke("NextLevel", roundEndPanelShowTime);
                }
            }
            else if (playerOne.GetIsDead() && playerTwo.GetIsDead()) // Draw
            {
                roundEnded = true;
                totalRounds++;
                PlayerPrefs.SetInt("totalRounds", totalRounds);

                roundPanel.SetActive(true);
                roundText.text = "Draw";

                Invoke("RestartLevel", roundEndPanelShowTime);
            }
        }
    }

    private void HideRoundsPanel()
    {
        roundPanel.SetActive(false);
    }

    public void MainMenu(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void Rematch(string sceneToLoad)
    {
        PlayerPrefs.SetInt("roundsPlayerOne", 0);
        PlayerPrefs.SetInt("roundsPlayerTwo", 0);
        PlayerPrefs.SetInt("totalRounds", 1);
        SceneManager.LoadScene(sceneToLoad);
    }
    public void NextLevel()
    {
        int groundTileIndex = Random.Range(0, 8);
        PlayerPrefs.SetInt("groundTileIndex", groundTileIndex);
        float seed = Random.Range(0, 100000);
        PlayerPrefs.SetFloat("seed", seed);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
