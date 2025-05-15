using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Represents an animated enemy in the game, inheriting from the base Enemy class.
/// This class handles initialization of enemy-specific components such as the EnemyManager
/// and EnemyAnimator, and provides functionality for enemy animations and events.
/// </summary>
public class AnimatedEnemy : Enemy
{
    /// <summary>
    /// Reference to the EnemyAnimator component, which handles animation-related events.
    /// </summary>
    protected EnemyAnimator _enemyAnimator;

    /// <summary>
    /// Reference to the EnemyManager component, which manages enemy-specific logic and events.
    /// </summary>
    protected EnemyManager _enemyManager;

    /// <summary>
    /// Initializes the enemy with the specified detection range and target layer mask.
    /// This method also initializes the EnemyManager and EnemyAnimator components.
    /// </summary>
    /// <param name="detectionRange">The range within which the enemy can detect targets.</param>
    /// <param name="targetLayerMask">The layer mask used to identify valid targets.</param>
    protected override void InitializeEnemy(int detectionRange, LayerMask targetLayerMask)
    {
        base.InitializeEnemy(detectionRange, targetLayerMask);

        // Initialize the EnemyManager component
        _enemyManager = GetComponent<EnemyManager>();
        if (_enemyManager != null)
        {
            _enemyManager.Initialize();
        }
        else
        {
            Debug.LogError("EnemyManager is not set in the inspector");
        }

        // Initialize the EnemyAnimator component and bind its events
        _enemyAnimator = GetComponentInChildren<EnemyAnimator>();
        _enemyAnimator.InitializeEvents(_enemyManager.enemyEvents);
    }

    /// <summary>
    /// Handles aiming at the target by triggering the appropriate enemy event.
    /// </summary>
    protected virtual void AimAtTarget()
    {
        _enemyManager.enemyEvents.EnemyAim();
    }
}