using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float _timer = 180f;
    public TextMeshProUGUI timer;

    private float currentTime;
    private bool isRunning;

    void Start()
    {
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

        if (_timer <= 0)
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
    }

    void OnTimerEnd()
    {
        timer.text = "0:00";
    }
}
