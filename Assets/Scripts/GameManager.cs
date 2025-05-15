using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI endTimeText;
    [SerializeField] private TextMeshProUGUI endScoreText;
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private GameObject endGameUI;
    [SerializeField] private GameObject bikeObject;
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI newPointsPrefab;
    [SerializeField] private GameObject canvas;
    private int newPointsMaxRotation = 15;
    private Rigidbody bikerb;
    private BikeController bikeContr;
    private float timeElapsed;
    private int score = 0;
    private int targetCount = 0;
    private float countdownTime = 3f;
    private bool isFinished = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Target[] targets = FindObjectsByType<Target>(FindObjectsSortMode.None);
        targetCount = targets.Length;
        endGameUI.SetActive(false);
        bikerb = bikeObject.GetComponent<Rigidbody>();
        bikeContr = bikeObject.GetComponent<BikeController>();
        countDownText.gameObject.SetActive(true);
        Time.timeScale = 0f;
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        while (countdownTime > 0)
        {
            countDownText.text = Mathf.Ceil(countdownTime).ToString();
            countdownTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        countDownText.text = "Go!";
        yield return new WaitForSecondsRealtime(1f);
        countDownText.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!isFinished)
        {
            timeElapsed += Time.deltaTime;
        }
        UpdateTimerDisplay();
        UpdateScoreDisplay();
        if(targetCount == 0)
        {
            Invoke("EndGame", 1);
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

    public void AddScore(int points, string comment)
    {
        score += points;
        string info = "+" + points.ToString() + " " + comment;
        ShowAddedPoints(info);
    }

    void ShowAddedPoints(string message)
    {
        int xOffset = Random.Range(-300, 300);
        int yOffset = Random.Range(-200, 200);
        int rotation = Random.Range(-newPointsMaxRotation, newPointsMaxRotation);
        TextMeshProUGUI text = Instantiate(newPointsPrefab, canvas.transform);
        Vector3 localPos = text.transform.localPosition;
        localPos += new Vector3(xOffset, yOffset, 0);
        text.transform.localPosition = localPos;
        text.transform.localRotation = Quaternion.Euler(0, 0, rotation);
        text.text = message;
    }

    public void TargetKilled()
    {
        targetCount--;
        AddScore(100, "Target killed");
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