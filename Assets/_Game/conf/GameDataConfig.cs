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

public class GameStartConfig : MonoBehaviour
{
    public int PlayerAmount;

    public static GameStartConfig Instance { get; private set; }

    public event Action<int> OnPlayerConnected;
    public event Action<int> OnRequiredPlayersReached;

    private int _connectedPlayers = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optionally, if you want this to persist across scene loads:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError("Multiple GameStartConfig instances found!");
            Destroy(gameObject);
        }
    }

    public void IncrementConnectedPlayers()
    {
        _connectedPlayers++;
        OnPlayerConnected?.Invoke(_connectedPlayers);
        if (_connectedPlayers >= PlayerAmount)
        {
            OnRequiredPlayersReached?.Invoke(PlayerAmount);
        }
    }

    public int GetConnectedPlayers()
    {
        return _connectedPlayers;
    }
}