using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class EnemySpawnerManager : NetworkBehaviour
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

    void Start()
    {
        if (IsServer)
        {
            OnServerStarted();
        }
    }


    private void OnServerStarted()
    {
        //  ApplyExistingEnemyArrows();
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
                SpawnRogueEnemy(spawnPosition);
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
                SpawnSlimeEnemy(spawnPosition);
                _spawnedSlimeEnemyCount++;
                _SlimeEnemySpawnTimer = _SlimeEnemySpawnCooldown;
            }

            _SlimeEnemySpawnTimer -= Time.fixedDeltaTime; // Use Time.fixedDeltaTime for physics updates
            yield return new WaitForFixedUpdate(); // Wait for the next physics update
        }
    }

    private void SpawnRogueEnemy(Vector3 spawnPosition)
    {
        if (_rogueEnemyPrefab != null)
        {
            GameObject spawnedEnemy =
                Instantiate(_rogueEnemyPrefab, spawnPosition, Quaternion.identity, gameObject.transform);
            spawnedEnemy.GetComponent<NetworkObject>().Spawn();
            TargetingManager.Instance.AddEnemyToTargetingList(spawnedEnemy.GetComponent<Enemy>());
        }
    }

    private void SpawnSlimeEnemy(Vector3 spawnPosition)
    {
        if (_slimeEnemyPrefab != null)
        {
            GameObject spawnedEnemy =
                Instantiate(_slimeEnemyPrefab, spawnPosition, Quaternion.identity, gameObject.transform);
            spawnedEnemy.GetComponent<NetworkObject>().Spawn();
            TargetingManager.Instance.AddEnemyToTargetingList(spawnedEnemy.GetComponent<Enemy>());
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
}