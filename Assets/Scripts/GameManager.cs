
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;

    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text gameOverHighScoreText;

    private const string HighScoreKey = "HighScore";

 
    private static bool startGameplayOnLoad;

    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    public int Score { get; private set; }
    public int HighScore { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Time.timeScale = 1f;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (mainMenuPanel != null)
        {
            if (startGameplayOnLoad)
            {
                startGameplayOnLoad = false;
                mainMenuPanel.SetActive(false);
                Time.timeScale = 1f;
            }
            else
            {
                mainMenuPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }
        else
        {
            Debug.LogWarning(
                "GameManager: MainMenuPanel is not assigned."
            );

            startGameplayOnLoad = false;
            Time.timeScale = 1f;
        }

        UpdateScoreUI();
        UpdateHighScoreUI();
    }

    public void StartGame()
    {
        if (mainMenuPanel == null)
        {
            Debug.LogWarning("MainMenuPanel is not assigned!");
            return;
        }

        startGameplayOnLoad = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void AddScore(int amount)
    {
        if (IsGameOver)
            return;

        Score += amount;

        if (Score > HighScore)
        {
            HighScore = Score;

            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }

        UpdateScoreUI();
        UpdateHighScoreUI();

        Debug.Log("Score: " + Score);
        Debug.Log("High Score: " + HighScore);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + Score;

        if (gameOverScoreText != null)
            gameOverScoreText.text = "Score: " + Score;
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
            highScoreText.text = "High Score: " + HighScore;

        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = "High Score: " + HighScore;
    }

    public void GameOver()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;

        UpdateScoreUI();
        UpdateHighScoreUI();

        Debug.Log("GAME OVER");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void PauseGame()
    {
        if (IsGameOver || IsPaused)
            return;

        if (pausePanel == null)
        {
            Debug.LogWarning("PausePanel is not assigned!");
            return;
        }

        pausePanel.SetActive(true);
        IsPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (IsGameOver)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        IsPaused = false;
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        startGameplayOnLoad = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ReturnToMainMenu()
    {
        startGameplayOnLoad = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game requested.");

        Application.Quit();
    }
}
































































//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections;
//using TMPro;

//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance { get; private set; }

//    [Header("UI")]
//    [SerializeField] private GameObject gameOverPanel;
//    [SerializeField] private GameObject pausePanel;

//    [SerializeField] private TMP_Text scoreText;
//    [SerializeField] private TMP_Text highScoreText;

//    [SerializeField] private TMP_Text gameOverScoreText;
//    [SerializeField] private TMP_Text gameOverHighScoreText;

//    private const string HighScoreKey = "HighScore";

//    public bool IsGameOver { get; private set; }
//    public bool IsPaused { get; private set; }

//    public int Score { get; private set; }

//    public int HighScore { get; private set; }

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;

//        Time.timeScale = 1f;

//        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
//    }

//    private void Start()
//    {
//        if (gameOverPanel != null)
//        {
//            gameOverPanel.SetActive(false);
//        }

//        if (pausePanel != null)
//        {
//            pausePanel.SetActive(false);
//        }

//        UpdateScoreUI();
//        UpdateHighScoreUI();
//    }

//    public void AddScore(int amount)
//    {
//        if (IsGameOver)
//            return;

//        Score += amount;

//        if (Score > HighScore)
//        {
//            HighScore = Score;

//            PlayerPrefs.SetInt(HighScoreKey, HighScore);
//            PlayerPrefs.Save();
//        }

//        UpdateScoreUI();
//        UpdateHighScoreUI();

//        Debug.Log("Score: " + Score);
//        Debug.Log("High Score: " + HighScore);
//    }

//    private void UpdateScoreUI()
//    {
//        if (scoreText != null)
//        {
//            scoreText.text = "Score: " + Score;
//        }

//        if (gameOverScoreText != null)
//        {
//            gameOverScoreText.text = "Score: " + Score;
//        }
//    }

//    private void UpdateHighScoreUI()
//    {
//        if (highScoreText != null)
//        {
//            highScoreText.text = "High Score: " + HighScore;
//        }

//        if (gameOverHighScoreText != null)
//        {
//            gameOverHighScoreText.text = "High Score: " + HighScore;
//        }
//    }

//    public void GameOver()
//    {
//        if (IsGameOver)
//            return;

//        IsGameOver = true;

//        //update them
//        UpdateScoreUI();
//        UpdateHighScoreUI();

//        Debug.Log("GAME OVER");

//        if (gameOverPanel != null)
//        {
//            gameOverPanel.SetActive(true);
//        }

//        Time.timeScale = 0f;
//    }

//    public void PauseGame()
//    {
//        if (IsGameOver || IsPaused)
//            return;

//        if (pausePanel == null)
//        {
//            Debug.LogWarning("PausePanel is not assigned!");
//            return;
//        }

//        pausePanel.SetActive(true);
//        IsPaused = true;
//        Time.timeScale = 0f;
//    }

//    public void ResumeGame()
//    {
//        if (IsGameOver)
//            return;

//        if (pausePanel != null)
//        {
//            pausePanel.SetActive(false);
//        }

//        IsPaused = false;
//        Time.timeScale = 1f;
//    }

//    public void RestartGame()
//    {
//        StartCoroutine(RestartRoutine());
//    }

//    private IEnumerator RestartRoutine()
//    {
//        yield return null;

//        Time.timeScale = 1f;

//        SceneManager.LoadScene(
//            SceneManager.GetActiveScene().buildIndex
//        );
//    }
//}



























































