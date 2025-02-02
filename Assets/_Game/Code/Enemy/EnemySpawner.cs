using Unity.Netcode;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    // Update is called once per frame
    void OnDestroy()
    {
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }

    [ServerRpc]
    private void OnServerStarted()
    {
        SpawnEnemyArrows();
    }

    private void SpawnEnemyArrows()
    {
        var enemiesTransform = GameObject.Find("Enemies")?.transform;
        if (enemiesTransform != null)
        {
            foreach (Transform child in enemiesTransform)
            {
                var enemy = child.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.InstantiateArrowServerRpc();
                }
            }
        }
    }
}