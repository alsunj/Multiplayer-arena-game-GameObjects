using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TargetingManager : NetworkBehaviour
{
    public static TargetingManager Instance { get; private set; }
    private Collider[] playerColliders;
    private List<Enemy> enemies = new List<Enemy>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            playerColliders = new Collider[NetworkManager.Singleton.ConnectedClients.Count];
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            SearchForPlayersInEnemyRadius();
        }
    }

    private void SearchForPlayersInEnemyRadius()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                int numColliders = Physics.OverlapSphereNonAlloc(enemy.transform.position,
                    enemy.DetectionRadius,
                    playerColliders, enemy.TargetLayerMask);

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

    public void AddEnemyToTargetingList(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    public void RemoveEnemyFromTargetingList(Enemy enemy)
    {
        enemies.Remove(enemy);
    }

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