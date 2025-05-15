using System;

/// <summary>
/// Manages events related to enemy actions such as attacking, aiming, and reloading.
/// Provides methods to trigger these events and allows other components to subscribe to them.
/// </summary>
public class EnemyEvents
{
    /// <summary>
    /// Event triggered when an enemy performs an attack.
    /// </summary>
    public event Action onEnemyAttack;

    /// <summary>
    /// Event triggered when an enemy starts aiming.
    /// </summary>
    public event Action onEnemyAim;

    /// <summary>
    /// Event triggered when an enemy reloads its weapon.
    /// </summary>
    public event Action onEnemyReload;

    /// <summary>
    /// Triggers the onEnemyAttack event to notify subscribers that an enemy has attacked.
    /// </summary>
    public void EnemyAttack()
    {
        if (onEnemyAttack != null)
        {
            onEnemyAttack();
        }
    }

    /// <summary>
    /// Triggers the onEnemyAim event to notify subscribers that an enemy has started aiming.
    /// </summary>
    public void EnemyAim()
    {
        if (onEnemyAim != null)
        {
            onEnemyAim();
        }
    }

    /// <summary>
    /// Triggers the onEnemyReload event to notify subscribers that an enemy has reloaded its weapon.
    /// </summary>
    public void EnemyReload()
    {
        if (onEnemyReload != null)
        {
            onEnemyReload();
        }
    }
}