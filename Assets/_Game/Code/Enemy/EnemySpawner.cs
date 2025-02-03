using Unity.Netcode;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStartedServerRpc;
        NetworkManager.Singleton.OnServerStopped += OnServerStoppedServerRpc;
    }

    [ServerRpc]
    private void OnServerStoppedServerRpc(bool state)
    {
        if (!state)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStartedServerRpc;
            NetworkManager.Singleton.OnServerStopped -= OnServerStoppedServerRpc;
        }
    }

    [ServerRpc]
    private void OnServerStartedServerRpc()
    {
        //  SpawnEnemyArrows();

        var enemiesTransform = GameObject.Find("Enemies")?.transform;
        if (enemiesTransform != null)
        {
            foreach (Transform child in enemiesTransform)
            {
                var enemy = child.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.InstantiateArrowServer();
                }
            }
        }
    }

    // private void SpawnEnemyArrows()
    // {
    //     var enemiesTransform = GameObject.Find("Enemies")?.transform;
    //     if (enemiesTransform != null)
    //     {
    //         foreach (Transform child in enemiesTransform)
    //         {
    //             var enemy = child.GetComponent<Enemy>();
    //             if (enemy != null)
    //             {
    //                 enemy.InstantiateArrowServer();
    //             }
    //         }
    //     }
    // }
}