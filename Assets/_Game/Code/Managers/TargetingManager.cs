using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TargetingManager : NetworkBehaviour
{
    public static TargetingManager Instance { get; private set; }
    private Collider[] playerColliders;
    private List<Enemy> enemies;
1
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
                int numColliders = Physics.OverlapSphereNonAlloc(enemy.gameObject.transform.position,
                    enemy.DetectionRadius,
                    playerColliders, enemy.TargetLayerMask);

                if (numColliders > 0)
                {
                    for (int i = 0; i < numColliders; i++)
                    {
                        enemy.Target = playerColliders[i].gameObject.transform;
                    }
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
        DontDestroyOnLoad(gameObject);
    }
}