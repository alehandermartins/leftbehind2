using UnityEngine;
using UnityEngine.Events;

public class Timer
{
    public float maxTime;
    public float time;
    private bool enabled = true;
    private bool finished = false;
    public bool main = false;
    public UnityEvent timedEvent = new UnityEvent();

    public Timer(float time, UnityEngine.Events.UnityAction callback)
    {
        this.maxTime = time;
        this.time = time;

        if(!(callback is null))
            timedEvent.AddListener(callback);
    }

    public void Reset()
    {
        time = maxTime;
        enabled = true;
        finished = false;
    }

    public void CountDown()
    {
        if (!enabled || finished)
            return;

        time -= Time.deltaTime;
        if (time <= 0f)
        {
            time = 0f;
            finished = true;
            timedEvent.Invoke();
        }
    }

    public bool Finished()
    {
        return time == 0f;
    }

    public void Enable(bool status)
    {
        enabled = status;
    }

    public bool IsEnabled()
    {
        return enabled;
    }
}
