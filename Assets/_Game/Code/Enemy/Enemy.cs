using System;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private EnemySettings enemySettings;
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject weapon;
    private EnemyManager _enemyManager;
    private EnemyAnimator _enemyAnimator;
    private NetworkObject _instantiatedArrow;
    private Transform _arrowSpawnPoint;
    private float _shootingTimer;
    private bool _isCrossbowLoaded;
    private bool _isAiming;
    private Vector3 _lookingDirection;
    private bool _targetLocked;
    private Collider[] hitColliders;

    public void InitializeEnemy()
    {
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
        if (_enemyAnimator != null)
        {
            _enemyAnimator.InitializeEvents(_enemyManager.enemyEvents);
            _enemyAnimator.receiveTargetShotEventFromAnimator += TargetShotAnimationEndEventServerRpc;
            _enemyAnimator.receiveTargetAimedEventFromAnimator += ShootTargetServerRpc;
            _enemyAnimator.receiveTargetReloadEventFromAnimator += ReloadCrossbowServerRpc;
        }
        else
        {
            Debug.LogError("EnemyAnimator is not found as a child");
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

        InstantiateArrowServer();

        hitColliders = new Collider[NetworkManager.Singleton.ConnectedClients.Count];
    }


    public void InstantiateArrowServer()
    {
        _instantiatedArrow = Instantiate(arrow, _arrowSpawnPoint.position, _arrowSpawnPoint.rotation)
            .GetComponent<NetworkObject>();
        _instantiatedArrow.Spawn();
        _instantiatedArrow.GetComponent<Arrow>().SetTargetTransform(_arrowSpawnPoint.transform);
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

        if (!_targetLocked)
        {
            ScanForCollisionServerRpc();
        }
    }


    [ServerRpc]
    public void TargetShotAnimationEndEventServerRpc()
    {
        _instantiatedArrow.GetComponent<Arrow>().RemoveTargetTransform();
        _instantiatedArrow.GetComponent<Rigidbody>().linearVelocity = _lookingDirection * enemySettings.shootingRange;
        //UpdateArrowTransformClientRpc(new NetworkObjectReference(_instantiatedArrow), _arrowSpawnPoint.position,
        //    _arrowSpawnPoint.rotation);
        _enemyManager.enemyEvents.EnemyReload();
    }

    [ServerRpc]
    private void ReloadCrossbowServerRpc()
    {
        //_instantiatedArrow.gameObject.SetActive(false);
        //  _instantiatedArrow.transform.SetParent(weapon.transform);
        _instantiatedArrow.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        _instantiatedArrow.GetComponent<Arrow>().SetTargetTransform(_arrowSpawnPoint.transform);
        _targetLocked = false;
        _isCrossbowLoaded = true;
    }


    [ServerRpc]
    private void ScanForCollisionServerRpc()
    {
        if (_isCrossbowLoaded && _shootingTimer <= 0)
        {
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, enemySettings.detectionRange,
                hitColliders,
                enemySettings.targetLayer);
            if (numColliders > 0)
            {
                Collider closestCollider = null;
                float closestDistance = float.MaxValue;

                for (int i = 0; i < numColliders; i++)
                {
                    float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestCollider = hitColliders[i];
                    }
                }

                _targetLocked = true;
                // RotateTowardsTargetClientRpc(closestCollider.gameObject.GetComponent<NetworkObject>().NetworkObjectId);

                GameObject targetObject = closestCollider.gameObject;
                _enemyManager.enemyEvents.EnemyAim();
                _lookingDirection = (targetObject.transform.position - transform.position).normalized;
                Quaternion lookRotation =
                    Quaternion.LookRotation(new Vector3(_lookingDirection.x, 0, _lookingDirection.z));
                transform.DORotateQuaternion(lookRotation, 0.5f);
            }
        }
    }

    [ServerRpc]
    private void ShootTargetServerRpc()
    {
        _shootingTimer = enemySettings.shootingDelay;
        _isCrossbowLoaded = false;
        _enemyManager.enemyEvents.EnemyAttack();
    }
}