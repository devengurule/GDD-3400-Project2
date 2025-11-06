using UnityEngine;

public class Timer : MonoBehaviour
{
    private float duration;
    private float currentTime;
    private float timeLeft;
    private bool running = false;


    public float GetDurration()
    {
        return duration;
    }
    public float GetTimeLeft()
    {
        return timeLeft;
    }
    private void SetDuration(float duration)
    {
        this.duration = duration;
    }
    public bool IsRunning()
    {
        return running;
    }
    public void Run(float duration)
    {
        if(duration == 0)
        {
            running = false;
            return;
        }
        running = true;
        currentTime = 0;
        SetDuration(duration);
    }
    private void Update()
    {
        if(running)
        {
            currentTime += Time.deltaTime;
            if (timeLeft > 0) timeLeft = duration - currentTime;
            else timeLeft = 0;
            if(currentTime >= duration)
            {
                running = false;
                currentTime = 0;
            }
        }
    }
}
