using System;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Rogue : AnimatedEnemy
{
    [SerializeField] private EnemySettings enemySettings;
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject weapon;
    private Arrow _arrowComponent;
    private float _shootingCooldown;
    private bool _isAiming;
    private Vector3 _lookingDirection;
    private NetworkObject _instantiatedArrow;
    private Transform _arrowSpawnPoint;
    private Rigidbody _arrowRigidbody;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeEnemy(enemySettings.detectionRange, enemySettings.targetLayer);
        }
    }

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


    public void InstantiateArrow()
    {
        _instantiatedArrow = Instantiate(arrow, _arrowSpawnPoint.position, _arrowSpawnPoint.rotation)
            .GetComponent<NetworkObject>();
        _instantiatedArrow.Spawn();
        _arrowComponent = _instantiatedArrow.GetComponent<Arrow>();
        _arrowRigidbody = _instantiatedArrow.GetComponent<Rigidbody>();
        _arrowComponent.SetTargetTransform(_arrowSpawnPoint.transform);
    }

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

    protected override void AimAtTarget()
    {
        if (!_instantiatedArrow.gameObject.activeSelf)
        {
            _enemyManager.enemyEvents.EnemyReload();
            return;
        }

        base.AimAtTarget();
    }

    private void RotateTowardsTarget()
    {
        _lookingDirection = (Target.position - transform.position).normalized;
        Quaternion lookRotation =
            Quaternion.LookRotation(new Vector3(_lookingDirection.x, 0, _lookingDirection.z));
        transform.DORotateQuaternion(lookRotation, 0.5f);
        AimAtTarget();
    }

    private void ShootTarget()
    {
        _enemyManager.enemyEvents.EnemyAttack();
        _shootingCooldown = enemySettings.shootingDelay;
    }

    private void TargetShotEvent()
    {
        _enemyManager.enemyEvents.EnemyReload();
        _arrowComponent.RemoveTargetTransform();
        _arrowRigidbody.linearVelocity = _lookingDirection * enemySettings.shootingRange;
    }

    private void ReloadCrossbow()
    {
        _arrowRigidbody.linearVelocity = Vector3.zero;
        _arrowComponent.SetTargetTransform(_arrowSpawnPoint.transform);
        NetworkObjectReference arrowReference = _instantiatedArrow;
        EnableArrowClientRpc(arrowReference);
    }


    [ClientRpc]
    private void EnableArrowClientRpc(NetworkObjectReference arrowReference)
    {
        if (arrowReference.TryGet(out NetworkObject arrowToEnable))
        {
            arrowToEnable.gameObject.SetActive(true);
        }
    }
}