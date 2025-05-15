using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages the targeting system for enemies, allowing them to detect and target players within a specified radius.
/// </summary>
public class TargetingManager : NetworkBehaviour
{
    /// <summary>
    /// Singleton instance of the TargetingManager.
    /// </summary>
    public static TargetingManager Instance { get; private set; }

    private Collider[] playerColliders; // Array to store colliders of detected players.
    private List<Enemy> enemies = new List<Enemy>(); // List of enemies managed by the targeting system.

    /// <summary>
    /// Called when the object is spawned on the network. Initializes player colliders on the server.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            playerColliders = new Collider[NetworkManager.Singleton.ConnectedClients.Count];
        }
    }

    /// <summary>
    /// Updates the targeting system every frame. Searches for players in the detection radius of each enemy.
    /// </summary>
    private void Update()
    {
        if (IsServer)
        {
            SearchForPlayersInEnemyRadius();
        }
    }

    /// <summary>
    /// Searches for players within the detection radius of each enemy and assigns the closest player as the target.
    /// </summary>
    private void SearchForPlayersInEnemyRadius()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                int numColliders = Physics.OverlapSphereNonAlloc(
                    enemy.transform.position,
                    enemy.DetectionRadius,
                    playerColliders,
                    enemy.TargetLayerMask
                );

                if (numColliders > 0)
                {
                    Transform closestPlayer = null;
                    float closestDistanceSqr = Mathf.Infinity;

                    for (int i = 0; i < numColliders; i++)
                    {
                        float distanceSqr = (playerColliders[i].transform.position - enemy.transform.position)
                            .sqrMagnitude;

                        if (distanceSqr < closestDistanceSqr)
                        {
                            closestDistanceSqr = distanceSqr;
                            closestPlayer = playerColliders[i].transform;
                        }
                    }

                    enemy.Target = closestPlayer;
                }
                else if (enemy.Target)
                {
                    enemy.Target = null;
                }
            }
        }
    }

    /// <summary>
    /// Adds an enemy to the targeting system's list of managed enemies.
    /// </summary>
    /// <param name="enemy">The enemy to add.</param>
    public void AddEnemyToTargetingList(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    /// <summary>
    /// Removes an enemy from the targeting system's list of managed enemies.
    /// </summary>
    /// <param name="enemy">The enemy to remove.</param>
    public void RemoveEnemyFromTargetingList(Enemy enemy)
    {
        enemies.Remove(enemy);
    }

    /// <summary>
    /// Ensures only one instance of TargetingManager exists and persists across scenes.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}