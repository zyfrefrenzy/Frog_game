using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gameplay")]
    public frog[] holes;               // drag all 9 frogs here
    public float gameDuration = 60f;
    public int startLives = 3;
    public float spawnInterval = 1.2f;
    public float minSpawnInterval = 0.45f;
    public float stayTime = 1.2f;
    public float minStayTime = 0.5f;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text livesText;         // optional, can be left empty
    public TMP_Text gameOverText;      // big centered text, hidden at start

    int score, lives;
    float timeLeft, currentInterval, currentStay;
    bool playing, gameOver;
    Camera cam;

    void Awake() { Instance = this; }

    void Start()
    {
        cam = Camera.main;
        StartGame();
    }

    public void StartGame()
    {
        foreach (var h in holes) h.ResetFrog();

        score = 0;
        lives = startLives;
        timeLeft = gameDuration;
        currentInterval = spawnInterval;
        currentStay = stayTime;
        playing = true;
        gameOver = false;
        gameOverText.gameObject.SetActive(false);   // hide Game Over
        UpdateUI();
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (!playing)
        {
            // after game over, click anywhere to play again
            if (gameOver && Input.GetMouseButtonDown(0)) StartGame();
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) { timeLeft = 0f; EndGame(); return; }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null && hit.TryGetComponent(out frog f))
                f.Hit();
            else
                score = Mathf.Max(0, score - 1);   // wrong click penalty
        }
        UpdateUI();
    }

    IEnumerator SpawnLoop()
    {
        while (playing)
        {
            frog h = holes[Random.Range(0, holes.Length)];
            if (!h.IsUp) h.Pop(currentStay);

            yield return new WaitForSeconds(currentInterval);

            currentInterval = Mathf.Max(minSpawnInterval, currentInterval - 0.02f);
            currentStay = Mathf.Max(minStayTime, currentStay - 0.01f);
        }
    }

    public void FrogKilled() { score += 10; }

    public void FrogMissed()
    {
        if (!playing) return;
        lives--;
        UpdateUI();
        if (lives <= 0) EndGame();
    }

    void EndGame()
    {
        if (!playing) return;
        playing = false;
        gameOver = true;
        StopAllCoroutines();
        foreach (var h in holes) h.ResetFrog();
        UpdateUI();

        // show Game Over on screen
        gameOverText.gameObject.SetActive(true);
        gameOverText.text = "GAME OVER\nScore: " + score + "\nClick to restart";
    }

    void UpdateUI()
    {
        scoreText.text = "Score:\n" + score;

        // format time like 0:49
        int t = Mathf.CeilToInt(timeLeft);
        timerText.text = "Time:\n" + (t / 60) + ":" + (t % 60).ToString("00");

        if (livesText) livesText.text = "Lives: " + lives;
    }
}