using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : Health
{
    public GameObject[] bars;

    public override void Damage()
    {
        base.Damage();
        bars[currentHealth].SetActive(false);
    }

    public override void Recover()
    {
        bars[currentHealth].SetActive(true);
        base.Recover();
    }

    public override void Empty()
    {
        base.Empty();
        foreach(GameObject bar in bars)
        {
            bar.SetActive(false);
        }
    }
}
