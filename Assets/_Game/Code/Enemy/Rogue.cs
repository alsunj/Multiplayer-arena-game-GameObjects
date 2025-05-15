using System;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Represents a Rogue enemy, inheriting from AnimatedEnemy.
/// This class handles the initialization and behavior of the Rogue, including aiming, shooting, and reloading.
/// </summary>
public class Rogue : AnimatedEnemy
{
    /// <summary>
    /// Settings for the enemy, including detection range, target layer, and shooting delay.
    /// </summary>
    [SerializeField] private EnemySettings enemySettings;

    /// <summary>
    /// Prefab for the arrow used by the Rogue.
    /// </summary>
    [SerializeField] private GameObject arrow;

    /// <summary>
    /// Reference to the weapon GameObject used by the Rogue.
    /// </summary>
    [SerializeField] private GameObject weapon;

    private Arrow _arrowComponent; // Reference to the Arrow component of the instantiated arrow.
    private float _shootingCooldown; // Cooldown timer for shooting.
    private bool _isAiming; // Indicates whether the Rogue is currently aiming.
    private Vector3 _lookingDirection; // Direction the Rogue is looking towards.
    private NetworkObject _instantiatedArrow; // Networked instance of the arrow.
    private Transform _arrowSpawnPoint; // Spawn point for the arrow.
    private Rigidbody _arrowRigidbody; // Rigidbody component of the arrow.

    /// <summary>
    /// Called when the Rogue is spawned on the network.
    /// Initializes the enemy with the specified settings.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeEnemy(enemySettings.detectionRange, enemySettings.targetLayer);
        }
    }

    /// <summary>
    /// Initializes the Rogue with the specified detection range and target layer mask.
    /// Sets up event listeners and validates required components.
    /// </summary>
    /// <param name="detectionRange">The range within which the Rogue can detect targets.</param>
    /// <param name="targetLayerMask">The layer mask used to identify valid targets.</param>
    protected override void InitializeEnemy(int detectionRange, LayerMask targetLayerMask)
    {
        base.InitializeEnemy(detectionRange, targetLayerMask);

        if (_enemyAnimator != null)
        {
            _enemyAnimator.receiveTargetShotEventFromAnimator += TargetShotEvent;
            _enemyAnimator.receiveTargetAimedEventFromAnimator += ShootTarget;
            _enemyAnimator.receiveTargetReloadEventFromAnimator += ReloadCrossbow;
        }
        else
        {
            Debug.LogError("EnemyAnimator, EnemyManager, or EnemyEvents not initialized in Rogue.");
        }

        if (enemySettings == null)
        {
            throw new Exception("EnemySettings is not set in the inspector");
        }

        if (weapon == null)
        {
            throw new Exception("Weapon is not set in the inspector");
        }

        _arrowSpawnPoint = weapon.transform.Find("Skeleton_Crossbow/ArrowSpawnPoint");
        if (_arrowSpawnPoint == null)
        {
            throw new Exception("ArrowSpawnPoint is not found as a child of Weapon");
        }

        if (arrow == null)
        {
            throw new Exception("Arrow is not set in the inspector");
        }

        InstantiateArrow();
    }

    /// <summary>
    /// Instantiates the arrow at the spawn point and sets up its components.
    /// </summary>
    public void InstantiateArrow()
    {
        _instantiatedArrow = Instantiate(arrow, _arrowSpawnPoint.position, _arrowSpawnPoint.rotation)
            .GetComponent<NetworkObject>();
        _instantiatedArrow.Spawn();
        _arrowComponent = _instantiatedArrow.GetComponent<Arrow>();
        _arrowRigidbody = _instantiatedArrow.GetComponent<Rigidbody>();
        _arrowComponent.SetTargetTransform(_arrowSpawnPoint.transform);
    }

    /// <summary>
    /// Updates the Rogue's behavior, including cooldown management and target rotation.
    /// </summary>
    private void Update()
    {
        if (!IsServer) return;

        if (_shootingCooldown > 0)
        {
            _shootingCooldown -= Time.deltaTime;
            return;
        }

        if (Target)
        {
            RotateTowardsTarget();
        }
    }

    /// <summary>
    /// Handles aiming at the target. If the arrow is inactive, triggers a reload.
    /// </summary>
    protected override void AimAtTarget()
    {
        if (!_instantiatedArrow.gameObject.activeSelf)
        {
            _enemyManager.enemyEvents.EnemyReload();
            return;
        }

        base.AimAtTarget();
    }

    /// <summary>
    /// Rotates the Rogue to face the target and triggers the aim action.
    /// </summary>
    private void RotateTowardsTarget()
    {
        _lookingDirection = (Target.position - transform.position).normalized;
        Quaternion lookRotation =
            Quaternion.LookRotation(new Vector3(_lookingDirection.x, 0, _lookingDirection.z));
        transform.DORotateQuaternion(lookRotation, 0.5f);
        AimAtTarget();
    }

    /// <summary>
    /// Handles shooting at the target and sets the shooting cooldown.
    /// </summary>
    private void ShootTarget()
    {
        _enemyManager.enemyEvents.EnemyAttack();
        _shootingCooldown = enemySettings.shootingDelay;
    }

    /// <summary>
    /// Handles the event when the target is shot. Reloads the crossbow and applies velocity to the arrow.
    /// </summary>
    private void TargetShotEvent()
    {
        _enemyManager.enemyEvents.EnemyReload();
        _arrowComponent.RemoveTargetTransform();
        _arrowRigidbody.linearVelocity = _lookingDirection * enemySettings.shootingRange;
    }

    /// <summary>
    /// Reloads the crossbow by resetting the arrow's position and enabling it on clients.
    /// </summary>
    private void ReloadCrossbow()
    {
        _arrowRigidbody.linearVelocity = Vector3.zero;
        _arrowComponent.SetTargetTransform(_arrowSpawnPoint.transform);
        NetworkObjectReference arrowReference = _instantiatedArrow;
        EnableArrowClientRpc(arrowReference);
    }

    /// <summary>
    /// Enables the arrow on all clients.
    /// </summary>
    /// <param name="arrowReference">A reference to the arrow's NetworkObject.</param>
    [ClientRpc]
    private void EnableArrowClientRpc(NetworkObjectReference arrowReference)
    {
        if (arrowReference.TryGet(out NetworkObject arrowToEnable))
        {
            arrowToEnable.gameObject.SetActive(true);
        }
    }
}