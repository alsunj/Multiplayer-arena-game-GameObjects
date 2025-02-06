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


    private Collider[]
        hitColliders = new Collider[10]; //TODO: this should be the amount of players connected to the network.

    private void Start()
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
            _enemyAnimator.receiveTargetShotEventFromAnimator += TargetShotEndEventServerRpc;
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

        if (arrow == null)
        {
            throw new Exception("Arrow is not set in the inspector");
        }

        if (weapon == null)
        {
            throw new Exception("Weapon is not set in the inspector");
        }

        _arrowSpawnPoint = weapon.transform.Find("ArrowSpawnPoint");
        if (_arrowSpawnPoint == null)
        {
            throw new Exception("ArrowSpawnPoint is not found as a child of Weapon");
        }
    }

    public void InstantiateArrowServer()
    {
        _instantiatedArrow = Instantiate(arrow, _arrowSpawnPoint.position, _arrowSpawnPoint.rotation)
            .GetComponent<NetworkObject>();
        _instantiatedArrow.Spawn();
        _instantiatedArrow.transform.SetParent(weapon.transform);
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


    [ServerRpc(RequireOwnership = false)]
    public void TargetShotEndEventServerRpc()
    {
        UpdateArrowTransformClientRpc(new NetworkObjectReference(_instantiatedArrow), _arrowSpawnPoint.position,
            _arrowSpawnPoint.rotation);
    }


    [ClientRpc]
    private void UpdateArrowTransformClientRpc(NetworkObjectReference arrowReference, Vector3 position,
        Quaternion rotation)
    {
        if (arrowReference.TryGet(out NetworkObject arrowObject))
        {
            // arrowObject.transform.position = position;
            // arrowObject.transform.rotation = rotation;
        }

        _enemyManager.enemyEvents.EnemyReload();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReloadCrossbowServerRpc()
    {
        _instantiatedArrow.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        // _instantiatedArrow.gameObject.SetActive(false);
        _instantiatedArrow.transform.position = _arrowSpawnPoint.position;
        _instantiatedArrow.transform.rotation = _arrowSpawnPoint.rotation;
        _instantiatedArrow.transform.SetParent(weapon.transform);
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

                RotateTowardsTargetClientRpc(closestCollider.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }

    [ClientRpc]
    private void RotateTowardsTargetClientRpc(ulong targetNetworkObjectId)
    {
        var targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        if (targetObject != null)
        {
            _enemyManager.enemyEvents.EnemyAim();
            _lookingDirection = (targetObject.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_lookingDirection.x, 0, _lookingDirection.z));
            transform.DORotateQuaternion(lookRotation, 0.5f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootTargetServerRpc()
    {
        _instantiatedArrow.transform.SetParent(null);
        _instantiatedArrow.transform.rotation = weapon.transform.rotation;
        _instantiatedArrow.GetComponent<Rigidbody>().linearVelocity = _lookingDirection * enemySettings.shootingRange;
        _shootingTimer = enemySettings.shootingDelay;
        _isCrossbowLoaded = false;
        ShootTargetClientRpc();
    }

    [ClientRpc]
    private void ShootTargetClientRpc()
    {
        _enemyManager.enemyEvents.EnemyAttack();
    }
}