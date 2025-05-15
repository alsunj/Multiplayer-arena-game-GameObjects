using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles enemy animation events and triggers animations for aiming, reloading, and attacking.
/// This class also integrates with the EnemyEvents system to respond to enemy-specific actions.
/// </summary>
public class EnemyAnimator : MonoBehaviour
{
    private const string IS_AIMING = "Aim"; // Animation trigger for aiming.
    private const string IS_RELOADING = "Reload"; // Animation trigger for reloading.
    private const string IS_ATTACKING = "Attack"; // Animation trigger for attacking.

    private EnemyEvents _enemyEvents; // Reference to the EnemyEvents instance.
    private Animator _animator; // Reference to the Animator component.

    /// <summary>
    /// Event triggered when the animator signals that the target has been shot.
    /// </summary>
    public event Action receiveTargetShotEventFromAnimator;

    /// <summary>
    /// Event triggered when the animator signals that the target has been aimed at.
    /// </summary>
    public event Action receiveTargetAimedEventFromAnimator;

    /// <summary>
    /// Event triggered when the animator signals that the target has been reloaded.
    /// </summary>
    public event Action receiveTargetReloadEventFromAnimator;

    /// <summary>
    /// Unsubscribes from EnemyEvents when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (_enemyEvents != null)
        {
            _enemyEvents.onEnemyAttack -= SetEnemyAttack;
            _enemyEvents.onEnemyAim -= SetEnemyAim;
            _enemyEvents.onEnemyReload -= SetEnemyReload;
        }
    }

    /// <summary>
    /// Initializes the EnemyAnimator with the provided EnemyEvents instance.
    /// Binds animation triggers to the corresponding enemy events.
    /// </summary>
    /// <param name="enemyEvents">The EnemyEvents instance to bind to.</param>
    public void InitializeEvents(EnemyEvents enemyEvents)
    {
        _animator = GetComponentInChildren<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator component not found in children.");
        }

        this._enemyEvents = enemyEvents;
        if (_enemyEvents != null)
        {
            _enemyEvents.onEnemyAttack += SetEnemyAttack;
            _enemyEvents.onEnemyAim += SetEnemyAim;
            _enemyEvents.onEnemyReload += SetEnemyReload;
        }
    }

    /// <summary>
    /// Invokes the event when the animator signals that the target has been aimed at.
    /// </summary>
    private void ReceiveTargetAimedEventFromAnimator()
    {
        receiveTargetAimedEventFromAnimator?.Invoke();
    }

    /// <summary>
    /// Invokes the event when the animator signals that the target has been reloaded.
    /// </summary>
    private void ReceiveTargetReloadEventFromAnimator()
    {
        receiveTargetReloadEventFromAnimator?.Invoke();
    }

    /// <summary>
    /// Invokes the event when the animator signals that the target has been shot.
    /// </summary>
    private void ReceiveTargetShotEventFromAnimator()
    {
        receiveTargetShotEventFromAnimator?.Invoke();
    }

    /// <summary>
    /// Triggers the reload animation for the enemy.
    /// </summary>
    private void SetEnemyReload()
    {
        _animator.SetTrigger(IS_RELOADING);
    }

    /// <summary>
    /// Triggers the aim animation for the enemy.
    /// </summary>
    private void SetEnemyAim()
    {
        _animator.SetTrigger(IS_AIMING);
    }

    /// <summary>
    /// Triggers the attack animation for the enemy.
    /// </summary>
    private void SetEnemyAttack()
    {
        _animator.SetTrigger(IS_ATTACKING);
    }
}