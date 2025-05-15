using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI endTimeText;
    [SerializeField] private TextMeshProUGUI endScoreText;
    [SerializeField] private GameObject endGameUI;
    private float timeElapsed;
    private int score = 0;
    private int targetCount = 0;

    void Start()
    {
        Target[] targets = FindObjectsByType<Target>(FindObjectsSortMode.None);
        targetCount = targets.Length;
        endGameUI.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        UpdateTimerDisplay();
        UpdateScoreDisplay();
        if(targetCount == 0)
        {
            EndGame();
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeElapsed / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void UpdateScoreDisplay()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public void TargetKilled()
    {
        targetCount--;
        AddScore(100);
    }

    void EndGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        endTimeText.text = timerText.text;
        endScoreText.text = scoreText.text;
        endGameUI.SetActive(true);
    }

    public void RestartGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}