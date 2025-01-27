using System;
using DG.Tweening;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemySettings enemySettings;
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject weapon;
    private GameObject _instantiatedArrow;
    private Transform _arrowSpawnPoint;
    private float _shootingTimer;
    private bool _isCrossbowLoaded;


    private Collider[]
        hitColliders = new Collider[10]; //TODO: this should be the amount of players connected to the network.

    private void Start()
    {
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
        }
        else if (!_isCrossbowLoaded)
        {
            ReloadArrow();
        }

        ScanForCollision();
    }

    private void ReloadArrow()
    {
        _instantiatedArrow.transform.SetParent(weapon.transform);
        _instantiatedArrow.transform.position = _arrowSpawnPoint.position;
        _instantiatedArrow.transform.rotation = _arrowSpawnPoint.rotation;
        _instantiatedArrow.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        _isCrossbowLoaded = true;
    }

    private void ScanForCollision()
    {
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, enemySettings.detectionRange, hitColliders,
            enemySettings.targetLayer);
        for (int i = 0; i < numColliders; i++)
        {
            if (_shootingTimer > 0)
            {
                RotateTowardsTarget(hitColliders[i]);
            }
            else
            {
                RotateTowardsTargetAndShootArrow(hitColliders[i]);
            }
        }
    }

    private void RotateTowardsTarget(Collider hitCollider)
    {
        Vector3 direction = (hitCollider.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.DORotateQuaternion(lookRotation, 0.7f);
    }

    private void RotateTowardsTargetAndShootArrow(Collider hitCollider)
    {
        Vector3 direction = (hitCollider.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.DORotateQuaternion(lookRotation, 0.7f).OnComplete(() =>
        {
            _instantiatedArrow.transform.SetParent(null);
            _instantiatedArrow.transform.rotation = weapon.transform.rotation;
            _instantiatedArrow.GetComponent<Rigidbody>().linearVelocity =
                direction * enemySettings.shootingRange;
            _shootingTimer = enemySettings.shootingDelay;
            _isCrossbowLoaded = false;
            //TODO: add delay for enemy to not rotate before the weapon is shot.
        });
    }
}