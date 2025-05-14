using UnityEngine;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    private float timeElapsed;
    private int score = 0;
    private int targetCount = 0;

    void Start()
    {
        Target[] targets = FindObjectsByType<Target>(FindObjectsSortMode.None);
        targetCount = targets.Length;
        Debug.Log(targetCount);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        UpdateTimerDisplay();
        UpdateScoreDisplay();
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
}