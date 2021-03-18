using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    public Slider slider;
    public Button btn;
    public bool inAction;

    public void SetMaxTime(float time)
    {
        slider.maxValue = time;
    }

    public void SetTime(float time)
    {
        slider.value = time;
    }

    public void Progress()
    {
        slider.value += Time.deltaTime;
    }

    public void Enable(bool status)
    {
        if (inAction)
            return;
        btn.interactable = status;
        SetTime(0f);
    }
}
