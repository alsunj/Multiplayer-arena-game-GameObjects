using System;
using DG.Tweening;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemySettings enemySettings;
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject weapon;
    private EnemyManager _enemyManager;
    private EnemyAnimator _enemyAnimator;
    private GameObject _instantiatedArrow;
    private Transform _arrowSpawnPoint;
    private float _shootingTimer;
    private bool _isCrossbowLoaded;
    private bool _isAiming;
    private Vector3 _LookingDirection;
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
            _enemyAnimator.receiveTargetAimedEventFromAnimator += ShootTarget;
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

        InstantiateArrow();
    }

    private void InstantiateArrow()
    {
        _instantiatedArrow = Instantiate(arrow, _arrowSpawnPoint.position, _arrowSpawnPoint.rotation);
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
        Debug.Log("Event received");
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
        _LookingDirection = (hitCollider.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_LookingDirection.x, 0, _LookingDirection.z));
        transform.DORotateQuaternion(lookRotation, 0.5f);
        //    .OnComplete(() => { _enemyManager.enemyEvents.EnemyAim(false); });
    }

    private void ShootTarget()
    {
        _instantiatedArrow.transform.SetParent(null);
        _instantiatedArrow.transform.rotation = weapon.transform.rotation;
        _instantiatedArrow.GetComponent<Rigidbody>().linearVelocity =
            _LookingDirection * enemySettings.shootingRange;
        _shootingTimer = enemySettings.shootingDelay;
        _isCrossbowLoaded = false;
        _enemyManager.enemyEvents.EnemyAttack();
    }
}