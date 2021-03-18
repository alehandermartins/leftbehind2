using System.Collections.Generic;
using UnityEngine;

public class TimeKeeper
{
    private Dictionary<string, Timer> timeBook = new Dictionary<string, Timer>();
    private List<Timer> disabled = new List<Timer>();

    public void CountDown()
    {
        foreach (Timer timer in timeBook.Values)
            timer.CountDown();
    }

    public void AddTimer(string name, float time, UnityEngine.Events.UnityAction callback = null)
    {
        Timer timer = new Timer(time, callback); 
        timeBook.Add(name, timer);
    }

    public void AddMainTimer(string name, float time, UnityEngine.Events.UnityAction callback = null)
    {
        Timer timer = new Timer(time, callback);
        timer.main = true;
        timeBook.Add(name, timer);
    }

    public float Get(string timer)
    {
        return timeBook[timer].time;
    }

    public void Reset(string timer, float time)
    {
        timeBook[timer].maxTime = time;
        timeBook[timer].Reset();
    }

    public void Increase(string name, float time)
    {
        Timer timer = timeBook[name];

        if (timer.time + time > timer.maxTime)
            timer.Reset();
        else
            timer.time += time;
    }

    public bool Finished(string timer)
    {
        return timeBook[timer].Finished();
    }

    public void Stop(string timer)
    {
        timeBook[timer].Enable(false);
    }

    public void Resume(string timer)
    {
        timeBook[timer].Enable(true);
    }

    public void Stop()
    {
        foreach (Timer timer in timeBook.Values)
        {
            if(!timer.main)
                timer.Enable(false);
        }
    }

    public void StopAll()
    {
        foreach (Timer timer in timeBook.Values)
        {
            timer.Enable(false);
        }
    }

    public void Disable()
    {
        foreach (Timer timer in timeBook.Values)
        {
            if (timer.IsEnabled())
            {
                disabled.Add(timer);
                timer.Enable(false);
            }
        }
    }

    public void Enable()
    {
        foreach (Timer timer in disabled)
            timer.Enable(true);

        disabled = new List<Timer>();
    }
}
