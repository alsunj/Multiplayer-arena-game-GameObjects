using UnityEngine;

/// <summary>
/// Represents a Slime enemy, inheriting from the base Enemy class.
/// This class handles the initialization and movement behavior of the Slime enemy.
/// </summary>
public class Slime : Enemy
{
    /// <summary>
    /// Settings for the Slime enemy, including detection range, target layer, and movement speed.
    /// </summary>
    [SerializeField] private EnemySettings enemySettings;

    /// <summary>
    /// Reference to the Rigidbody component used for controlling the Slime's movement.
    /// </summary>
    private Rigidbody _rigidbody;

    /// <summary>
    /// Layer mask for identifying valid target objects.
    /// </summary>
    private int _targetLayer;

    /// <summary>
    /// Called when the Slime is spawned on the network.
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
    /// Initializes the Slime with the specified detection range and target layer mask.
    /// Sets up the Rigidbody component for movement.
    /// </summary>
    /// <param name="detectionRange">The range within which the Slime can detect targets.</param>
    /// <param name="targetLayerMask">The layer mask used to identify valid targets.</param>
    protected override void InitializeEnemy(int detectionRange, LayerMask targetLayerMask)
    {
        base.InitializeEnemy(detectionRange, targetLayerMask);
        _rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Updates the Slime's movement behavior.
    /// Moves the Slime towards its target if a target is detected.
    /// </summary>
    private void Update()
    {
        if (Target)
        {
            _rigidbody.linearVelocity = (Target.position - transform.position).normalized * enemySettings.speed;
        }
    }
}