using System;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Enemy : MonoBehaviour
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
            _enemyAnimator.receiveTargetShotEventFromAnimator += TargetShotEndEvent;
            _enemyAnimator.receiveTargetAimedEventFromAnimator += ShootTargetServerRpc;
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

  if (NetworkManager.Singleton.IsListening)
    {
        InstantiateArrow();
    }
    else
    {
        Debug.LogError("NetworkManager is not listening, start a server or host before instantiating the arrow");
    }    }

    private void InstantiateArrow()
    {
        _instantiatedArrow = Instantiate(arrow, _arrowSpawnPoint.position, _arrowSpawnPoint.rotation).GetComponent<NetworkObject>();
        _instantiatedArrow.transform.SetParent(weapon.transform);
        _isCrossbowLoaded = true;
    }

    private void Update()
    {
        if (_shootingTimer > 0)
        {
            _shootingTimer -= Time.deltaTime;
            return;
        }

        if (!_targetLocked)
        {
            ScanForCollision();
        }
    }


    public void TargetShotEndEvent()
    {
        _instantiatedArrow.transform.SetParent(weapon.transform);
        _instantiatedArrow.transform.position = _arrowSpawnPoint.position;
        _instantiatedArrow.transform.rotation = _arrowSpawnPoint.rotation;
        _instantiatedArrow.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        _isCrossbowLoaded = true;
        _targetLocked = false;
    }

    private void ScanForCollision()
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

                RotateTowardsTarget(closestCollider);
            }
        }
    }

    private void RotateTowardsTarget(Collider hitCollider)
    {
        _enemyManager.enemyEvents.EnemyAim();
        _lookingDirection = (hitCollider.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_lookingDirection.x, 0, _lookingDirection.z));
        transform.DORotateQuaternion(lookRotation, 0.5f);
        
        //    .OnComplete(() => { _enemyManager.enemyEvents.EnemyAim(false); });
    }

    [ServerRpc]
    private void ShootTargetServerRpc()
    {
        ShootTarget();
    }

    private void ShootTarget()
    {
        _instantiatedArrow.transform.SetParent(null);
        _instantiatedArrow.transform.rotation = weapon.transform.rotation;
        _instantiatedArrow.GetComponent<Rigidbody>().linearVelocity = _lookingDirection * enemySettings.shootingRange;
        _shootingTimer = enemySettings.shootingDelay;
        _isCrossbowLoaded = false;
        _enemyManager.enemyEvents.EnemyAttack();
    }
}