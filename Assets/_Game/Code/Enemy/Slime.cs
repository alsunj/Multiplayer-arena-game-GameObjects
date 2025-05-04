using UnityEngine;

public class Slime : Enemy
{
    [SerializeField] private EnemySettings enemySettings;
    private Rigidbody _rigidbody;
    private int _targetLayer;


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
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Target)
        {
            _rigidbody.linearVelocity = (Target.position - transform.position).normalized * enemySettings.speed;
        }
    }
}