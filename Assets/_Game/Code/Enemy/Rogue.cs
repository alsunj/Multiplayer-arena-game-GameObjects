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
    private float _shootingTimer;
    private bool _isCrossbowLoaded;
    private bool _isAiming;
    private Vector3 _lookingDirection;
    private bool _targetLocked;
    private Collider[] hitColliders;

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

        hitColliders = new Collider[NetworkManager.Singleton.ConnectedClients.Count];
    }


    public void InstantiateArrow()
    {
        _instantiatedArrow = Instantiate(arrow, _arrowSpawnPoint.position, _arrowSpawnPoint.rotation)
            .GetComponent<NetworkObject>();
        _instantiatedArrow.Spawn();
        _arrowComponent = _instantiatedArrow.GetComponent<Arrow>();
        _arrowRigidbody = _instantiatedArrow.GetComponent<Rigidbody>();
        _arrowComponent.SetTargetTransform(_arrowSpawnPoint.transform);
        _isCrossbowLoaded = true;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (_shootingTimer > 0)
        {
            _shootingTimer -= Time.deltaTime;
            return;
        }

        if (Target)
        {
            RotateTowardsTarget();
        }
    }


    // [ServerRpc]
    // private void ScanForCollisionServerRpc()
    // {
    //     if (_isCrossbowLoaded && _shootingTimer <= 0)
    //     {
    //         int numColliders = Physics.OverlapSphereNonAlloc(transform.position, enemySettings.detectionRange,
    //             hitColliders,
    //             enemySettings.targetLayer);
    //         if (numColliders > 0)
    //         {
    //             Collider closestCollider = null;
    //             float closestDistance = float.MaxValue;
    //
    //             for (int i = 0; i < numColliders; i++)
    //             {
    //                 float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
    //                 if (distance < closestDistance)
    //                 {
    //                     closestDistance = distance;
    //                     closestCollider = hitColliders[i];
    //                 }
    //             }
    //
    //             _targetLocked = true;
    //             GameObject targetObject = closestCollider.gameObject;
    //             
    //         }
    //     }
    // }

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
        _shootingTimer = enemySettings.shootingDelay;
        _isCrossbowLoaded = false;
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
        _targetLocked = false;
        _isCrossbowLoaded = true;
    }
}