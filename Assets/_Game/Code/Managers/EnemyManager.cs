using UnityEngine;

/// <summary>
/// Manages enemy-related functionality in the game, including initializing enemy events.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    /// <summary>
    /// Handles events related to enemy actions such as attacking, aiming, and reloading.
    /// </summary>
    public EnemyEvents enemyEvents;

    /// <summary>
    /// Initializes the EnemyManager by creating a new instance of EnemyEvents.
    /// </summary>
    public void Initialize()
    {
        enemyEvents = new EnemyEvents();
    }
}