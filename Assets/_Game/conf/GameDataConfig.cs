using System;
using UnityEngine;

public class GameDataConfig : MonoBehaviour
{
    public static GameDataConfig Instance { get; private set; }

    public int RogueEnemyAmount;
    public int SlimeEnemyAmount;

    public float RogueEnemySpawnTimer = 2f;
    public float SlimeEnemySpawnTimer = 0.5f;

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