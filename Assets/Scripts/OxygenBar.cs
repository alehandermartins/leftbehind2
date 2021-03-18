using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OxygenBar : MonoBehaviour
{
    public Slider slider;
    public float maxOxygen = 5;

    void Start()
    {
        slider.maxValue = maxOxygen;
        slider.value = maxOxygen;
    }
        
    public void Set(float oxygen)
    {
        slider.value = oxygen;
    }

    public bool IsEmpty()
    {
        return slider.value == 0f;
    }

    public bool IsFull()
    {
        return slider.value == maxOxygen;
    }

    public float Missing()
    {
        return maxOxygen - slider.value;
    }
}
