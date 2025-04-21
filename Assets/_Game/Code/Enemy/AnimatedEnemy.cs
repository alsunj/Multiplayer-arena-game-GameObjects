using Unity.Netcode;
using UnityEngine;

public class AnimatedEnemy : Enemy
{
    protected EnemyAnimator _enemyAnimator;
    protected EnemyManager _enemyManager;

    protected override void InitializeEnemy()
    {
        base.InitializeEnemy();
        _enemyManager = GetComponent<EnemyManager>();
        if (_enemyManager != null)
        {
            _enemyManager.Initialize();
        }
        else
        {
            Debug.LogError("EnemyManager is not set in the inspector");
        }

        _enemyAnimator = GetComponentInChildren<EnemyAnimator>();
        _enemyAnimator.InitializeEvents(_enemyManager.enemyEvents);

        // if (_enemyAnimator != null)
        // {
        //     _enemyAnimator.InitializeEvents(_enemyManager.enemyEvents);
        //     _enemyAnimator.receiveTargetShotEventFromAnimator += TargetShotEventServerRpc;
        //     _enemyAnimator.receiveTargetAimedEventFromAnimator += ShootTargetServerRpc;
        //     _enemyAnimator.receiveTargetReloadEventFromAnimator += ReloadCrossbowServerRpc;
        // }
    }
    // not possible to raise the functions to inheritor
    // [ServerRpc]
    // protected virtual void ReloadCrossbowServerRpc()
    // {
    // }
    //
    // [ServerRpc]
    // protected virtual void ShootTargetServerRpc()
    // {
    //     _enemyManager.enemyEvents.EnemyAttack();
    // }
    //
    // [ServerRpc]
    // protected virtual void TargetShotEventServerRpc()
    // {
    //     _enemyManager.enemyEvents.EnemyReload();
    // }

    protected void AimAtTarget()
    {
        _enemyManager.enemyEvents.EnemyAim();
    }
}