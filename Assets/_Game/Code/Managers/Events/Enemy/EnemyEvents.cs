using System;

public class EnemyEvents
{
    public event Action onEnemyAttack;
    public event Action onEnemyAim;
    public event Action onEnemyReload;

    public void EnemyAttack()
    {
        if (onEnemyAttack != null)
        {
            onEnemyAttack();
        }
    }

    public void EnemyAim()
    {
        if (onEnemyAim != null)
        {
            onEnemyAim();
        }
    }

    public void EnemyReload()
    {
        if (onEnemyReload != null)
        {
            onEnemyReload();
        }
    }
}