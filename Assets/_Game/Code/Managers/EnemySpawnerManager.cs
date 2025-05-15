using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages the spawning of enemies in the game, including Rogue and Slime enemies.
/// Handles spawn timers, spawn locations, and enemy instantiation.
/// </summary>
public class EnemySpawnerManager : NetworkBehaviour
{
    /// <summary>
    /// Singleton instance of the EnemySpawnerManager.
    /// </summary>
    public static EnemySpawnerManager Instance { get; private set; }

    [SerializeField] private GameObject _rogueEnemyPrefab; // Prefab for Rogue enemies.
    [SerializeField] private GameObject _slimeEnemyPrefab; // Prefab for Slime enemies.

    private Transform[] _enemySpawnLocations; // Array of spawn locations for enemies.

    #region EnemyConfig

    private int _RougeEnemySpawnAmount; // Total number of Rogue enemies to spawn.
    private int _SlimeEnemySpawnAmount; // Total number of Slime enemies to spawn.
    private float _RogueEnemySpawnCooldown; // Cooldown time between Rogue enemy spawns.
    private float _SlimeEnemySpawnCooldown; // Cooldown time between Slime enemy spawns.

    #endregion

    #region runtimeEnemyProperties

    private float _RogueEnemySpawnTimer; // Timer for Rogue enemy spawning.
    private float _SlimeEnemySpawnTimer; // Timer for Slime enemy spawning.
    private int _spawnedRogueEnemyCount; // Count of spawned Rogue enemies.
    private int _spawnedSlimeEnemyCount; // Count of spawned Slime enemies.

    #endregion

    /// <summary>
    /// Ensures only one instance of EnemySpawnerManager exists and persists across scenes.
    /// </summary>
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

    /// <summary>
    /// Initializes the server-specific logic when the game starts.
    /// </summary>
    private void Start()
    {
        if (IsServer)
        {
            OnServerStarted();
        }
    }

    /// <summary>
    /// Sets up enemy spawn positions and configurations when the server starts.
    /// </summary>
    private void OnServerStarted()
    {
        FindEnemySpawnPositions();
        FindEnemyConfig();
    }

    /// <summary>
    /// Starts the spawn timers for Rogue and Slime enemies.
    /// </summary>
    public void StartEnemySpawnTimers()
    {
        StartCoroutine(RogueSpawnTimerCoroutine());
        StartCoroutine(SlimeSpawnTimerCoroutine());
    }

    /// <summary>
    /// Coroutine to handle Rogue enemy spawning based on the spawn timer.
    /// </summary>
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

            _RogueEnemySpawnTimer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    /// <summary>
    /// Coroutine to handle Slime enemy spawning based on the spawn timer.
    /// </summary>
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

            _SlimeEnemySpawnTimer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    /// <summary>
    /// Spawns a Rogue enemy at the specified position.
    /// </summary>
    /// <param name="spawnPosition">The position to spawn the Rogue enemy.</param>
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

    /// <summary>
    /// Spawns a Slime enemy at the specified position.
    /// </summary>
    /// <param name="spawnPosition">The position to spawn the Slime enemy.</param>
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

    /// <summary>
    /// Gets the spawn position for an enemy based on the spawn count.
    /// </summary>
    /// <param name="spawnedCount">The number of enemies already spawned.</param>
    /// <returns>The spawn position for the next enemy.</returns>
    private Vector3 GetSpawnPosition(int spawnedCount)
    {
        int index = spawnedCount % _enemySpawnLocations.Length;
        return _enemySpawnLocations[index].position;
    }

    /// <summary>
    /// Retrieves enemy configuration data from the game configuration.
    /// </summary>
    private void FindEnemyConfig()
    {
        _RougeEnemySpawnAmount = GameDataConfig.Instance.RogueEnemyAmount;
        _SlimeEnemySpawnAmount = GameDataConfig.Instance.SlimeEnemyAmount;
        _RogueEnemySpawnCooldown = GameDataConfig.Instance.RogueEnemySpawnTimer;
        _SlimeEnemySpawnCooldown = GameDataConfig.Instance.SlimeEnemySpawnTimer;
        _RogueEnemySpawnTimer = _RogueEnemySpawnCooldown;
        _SlimeEnemySpawnTimer = _SlimeEnemySpawnCooldown;
    }

    /// <summary>
    /// Finds and stores enemy spawn locations from the scene.
    /// </summary>
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
    }
}