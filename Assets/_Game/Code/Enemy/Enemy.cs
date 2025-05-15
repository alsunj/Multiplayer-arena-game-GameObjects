using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Represents a base class for enemies in the game.
/// Provides properties and methods for enemy initialization and behavior.
/// </summary>
public class Enemy : NetworkBehaviour
{
    /// <summary>
    /// Gets or sets the detection radius of the enemy.
    /// This defines the range within which the enemy can detect targets.
    /// </summary>
    public int DetectionRadius { get; set; }

    /// <summary>
    /// Gets or sets the current target of the enemy.
    /// </summary>
    public Transform Target { get; set; }

    /// <summary>
    /// Gets or sets the layer mask used to identify valid targets for the enemy.
    /// </summary>
    public LayerMask TargetLayerMask { get; set; }

    /// <summary>
    /// Initializes the enemy with the specified detection range and target layer mask.
    /// </summary>
    /// <param name="detectionRange">The range within which the enemy can detect targets.</param>
    /// <param name="targetLayerMask">The layer mask used to identify valid targets.</param>
    protected virtual void InitializeEnemy(int detectionRange, LayerMask targetLayerMask)
    {
        DetectionRadius = detectionRange;
        TargetLayerMask = targetLayerMask;
        Target = null;
    }

    // /// <summary>
    // /// Removes the enemy from the targeting list when it is destroyed.
    // /// </summary>
    // public override void OnDestroy()
    // {
    //     TargetingManager.Instance.RemoveEnemyFromTargetingList(this);
    // }
}