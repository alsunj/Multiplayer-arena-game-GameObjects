using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private const string IS_AIMING = "Aim";
    private const string IS_RELOADING = "Reload";
    private const string IS_ATTACKING = "Attack";

    private EnemyEvents _enemyEvents;
    private Animator _animator;

    public event Action receiveTargetShotEventFromAnimator;
    public event Action receiveTargetAimedEventFromAnimator;
    public event Action receiveTargetReloadEventFromAnimator;


    private void OnDisable()
    {
        if (_enemyEvents != null)
        {
            _enemyEvents.onEnemyAttack -= SetEnemyAttack;
            _enemyEvents.onEnemyAim -= SetEnemyAim;
            _enemyEvents.onEnemyReload -= SetEnemyReload;
        }
    }

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

    private void ReceiveTargetAimedEventFromAnimator()
    {
        Debug.Log("ReceiveTargetAimedEventFromAnimator");
        receiveTargetAimedEventFromAnimator?.Invoke();
    }

    private void ReceiveTargetReloadEventFromAnimator()
    {
        receiveTargetReloadEventFromAnimator?.Invoke();
    }

    private void ReceiveTargetShotEventFromAnimator()
    {
        Debug.Log("ReceiveTargetShotEventFromAnimator");
        receiveTargetShotEventFromAnimator?.Invoke();
    }

    private void SetEnemyReload()
    {
        _animator.SetTrigger(IS_RELOADING);
    }

    private void SetEnemyAim()
    {
        _animator.SetTrigger(IS_AIMING);
    }

    private void SetEnemyAttack()
    {
        _animator.SetTrigger(IS_ATTACKING);
    }
}