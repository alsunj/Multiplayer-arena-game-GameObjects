using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject _rogueEnemyPrefab;
    [SerializeField] private GameObject _slimeEnemyPrefab;

    private Transform[] _enemySpawnLocations;

    #region EnemyConfig

    private int _RougeEnemySpawnAmount;
    private int _SlimeEnemySpawnAmount;
    private float _RogueEnemySpawnCooldown;
    private float _SlimeEnemySpawnCooldown;

    #endregion

    #region runtimeEnemyProperties

    private float _RogueEnemySpawnTimer;
    private float _SlimeEnemySpawnTimer;
    private int _spawnedRogueEnemyCount;
    private int _spawnedSlimeEnemyCount;

    #endregion


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnServerStartedServerRpc();
        //NetworkManager.Singleton.OnServerStarted += OnServerStartedServerRpc;
        // NetworkManager.Singleton.OnServerStopped += OnServerStoppedServerRpc;
    }

    [ServerRpc]
    private void OnServerStoppedServerRpc(bool state)
    {
        if (!state)
        {
            //  NetworkManager.Singleton.OnServerStarted -= OnServerStartedServerRpc;
            //  NetworkManager.Singleton.OnServerStopped -= OnServerStoppedServerRpc;
        }
    }

    [ServerRpc]
    private void OnServerStartedServerRpc()
    {
        Debug.Log("Setting up enemy spawner");
        ApplyExistingEnemyArrows();
        FindEnemySpawnPositions();
        FindEnemyConfig();
        StartEnemySpawnTimers();
    }

    private void StartEnemySpawnTimers()
    {
        StartCoroutine(RogueSpawnTimerCoroutine());
        StartCoroutine(SlimeSpawnTimerCoroutine());
    }

    private IEnumerator RogueSpawnTimerCoroutine()
    {
        while (_spawnedRogueEnemyCount < _RougeEnemySpawnAmount)
        {
            if (_RogueEnemySpawnTimer <= 0f)
            {
                var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
                Vector3 spawnPosition = GetSpawnPosition(_spawnedRogueEnemyCount);
                spawnPosition += new Vector3(random.NextFloat(-5f, 5f), 0, random.NextFloat(-5f, 5f));
                SpawnRogueEnemyServerRpc(spawnPosition);
                _spawnedRogueEnemyCount++;
                _RogueEnemySpawnTimer = _RogueEnemySpawnCooldown;
            }

            _RogueEnemySpawnTimer -= Time.fixedDeltaTime; // Use Time.fixedDeltaTime for physics updates
            yield return new WaitForFixedUpdate(); // Wait for the next physics update
        }
    }

    private IEnumerator SlimeSpawnTimerCoroutine()
    {
        while (_spawnedSlimeEnemyCount < _SlimeEnemySpawnAmount)
        {
            if (_SlimeEnemySpawnTimer <= 0f)
            {
                var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
                Vector3 spawnPosition = GetSpawnPosition(_spawnedSlimeEnemyCount);
                spawnPosition += new Vector3(random.NextFloat(-5f, 5f), 0, random.NextFloat(-5f, 5f));
                SpawnSlimeEnemyServerRpc(spawnPosition);

                _spawnedSlimeEnemyCount++;
                _SlimeEnemySpawnTimer = _SlimeEnemySpawnCooldown;
            }

            _SlimeEnemySpawnTimer -= Time.fixedDeltaTime; // Use Time.fixedDeltaTime for physics updates
            yield return new WaitForFixedUpdate(); // Wait for the next physics update
        }
    }

    [ServerRpc]
    private void SpawnRogueEnemyServerRpc(Vector3 spawnPosition)
    {
        if (_rogueEnemyPrefab != null)
        {
            GameObject spawnedEnemy =
                Instantiate(_rogueEnemyPrefab, spawnPosition, Quaternion.identity, gameObject.transform);
            spawnedEnemy.GetComponent<NetworkObject>().Spawn();
            SpawnEnemyForClientRpc(spawnedEnemy);
        }
    }

    [ClientRpc]
    private void SpawnEnemyForClientRpc(GameObject spawnedEnemy)
    {
        spawnedEnemy.GetComponent<Enemy>().InitializeEnemy();
    }

    [ServerRpc]
    private void SpawnSlimeEnemyServerRpc(Vector3 spawnPosition)
    {
        if (_slimeEnemyPrefab != null)
        {
            GameObject spawnedEnemy = Instantiate(_slimeEnemyPrefab, spawnPosition, Quaternion.identity);
            spawnedEnemy.GetComponent<NetworkObject>().Spawn();
            //  spawnedEnemy.GetComponent<Enemy>().InitializeEnemy();
        }
    }

    private Vector3 GetSpawnPosition(int spawnedCount)
    {
        int index = spawnedCount % _enemySpawnLocations.Length;
        return _enemySpawnLocations[index].position;
    }


    private void FindEnemyConfig()
    {
        _RougeEnemySpawnAmount = GameDataConfig.Instance.RogueEnemyAmount;
        _SlimeEnemySpawnAmount = GameDataConfig.Instance.SlimeEnemyAmount;
        _RogueEnemySpawnCooldown = GameDataConfig.Instance.RogueEnemySpawnTimer;
        _SlimeEnemySpawnCooldown = GameDataConfig.Instance.SlimeEnemySpawnTimer;
        _RogueEnemySpawnTimer = _RogueEnemySpawnCooldown;
        _SlimeEnemySpawnTimer = _SlimeEnemySpawnCooldown;
    }

    private void ApplyExistingEnemyArrows()
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


    private void FindEnemySpawnPositions()
    {
        GameObject spawnLocationParent = GameObject.Find("EnemySpawnLocations");

        if (spawnLocationParent == null)
        {
            Debug.LogError("Could not find GameObject named 'EnemySpawnLocations'!");
            return;
        }

        _enemySpawnLocations = new Transform[spawnLocationParent.transform.childCount];

        for (int i = 0; i < spawnLocationParent.transform.childCount; i++)
        {
            _enemySpawnLocations[i] = spawnLocationParent.transform.GetChild(i);
        }

        Vector3[] spawnPositions = new Vector3[_enemySpawnLocations.Length];
        for (int i = 0; i < _enemySpawnLocations.Length; i++)
        {
            spawnPositions[i] = _enemySpawnLocations[i].position;
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