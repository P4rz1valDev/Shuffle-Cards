using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float _timer = 40f;
    public TextMeshProUGUI timer;
    public TextMeshProUGUI GameOver;

    private float currentTime;
    private bool isRunning;

    void Start()
    {
        GameOver.gameObject.SetActive(false);

        currentTime = _timer;
        isRunning = true;

        if (timer == null)
        {
            Debug.Log("Go fix it");
        }
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            isRunning = false;
            currentTime = 0;
            OnTimerEnd();
        }

        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        string newText = string.Format("{0}:{1:00}", minutes, seconds);

        timer.text = newText;

        if (currentTime <= 30f)
        {
            timer.color = Color.red;
        }
    }

    void OnTimerEnd()
    {
        Time.timeScale = 0f; 
        timer.text = "0:00";
        GameOver.gameObject.SetActive(true);
    }
}
