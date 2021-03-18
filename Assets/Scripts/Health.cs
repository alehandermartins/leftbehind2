using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth = 3;

    public virtual void Damage()
    {
        currentHealth--;
    }

    public virtual void Recover()
    {
        currentHealth++;
    }

    public bool IsFull()
    {
        return currentHealth == maxHealth;
    }

    public bool IsEmpty()
    {
        return currentHealth == 0;
    }

    public virtual void Empty()
    {
        currentHealth = 0;
    }
}
