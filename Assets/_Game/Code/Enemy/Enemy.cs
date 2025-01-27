using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemySettings enemySettings;

    private void Update()
    {
        ScanForCollision();
    }

    private void ScanForCollision()
    {
        Collider[] hitColliders =
            Physics.OverlapSphere(transform.position, enemySettings.shootingRange, enemySettings.targetLayer);
        foreach (var hitCollider in hitColliders)
        {
            Debug.Log("Collision detected with: " + hitCollider.name);
        }
    }
}