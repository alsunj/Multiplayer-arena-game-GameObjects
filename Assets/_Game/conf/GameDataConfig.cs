using System;
using UnityEngine;

/// <summary>
/// A singleton configuration class for managing game data related to enemy amounts and spawn timers.
/// </summary>
public class GameDataConfig : MonoBehaviour
{
    /// <summary>
    /// The singleton instance of the GameDataConfig class.
    /// </summary>
    public static GameDataConfig Instance { get; private set; }

    /// <summary>
    /// The number of rogue enemies in the game.
    /// </summary>
    public int RogueEnemyAmount;

    /// <summary>
    /// The number of slime enemies in the game.
    /// </summary>
    public int SlimeEnemyAmount;

    /// <summary>
    /// The spawn timer for rogue enemies.
    /// </summary>
    public float RogueEnemySpawnTimer;

    /// <summary>
    /// The spawn timer for slime enemies.
    /// </summary>
    public float SlimeEnemySpawnTimer;

    /// <summary>
    /// Ensures that only one instance of the GameDataConfig class exists.
    /// If another instance is found, it is destroyed.
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