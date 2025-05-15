using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages the respawn process for players, including UI updates, countdowns, and player reinitialization.
/// </summary>
public class RespawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject _respawnPlayerPanel; // UI panel displayed during respawn countdown.
    [SerializeField] private TextMeshProUGUI _countdownTimeText; // Text element for displaying countdown time.
    [SerializeField] private int _playerDeadLayer = 11; // Layer assigned to players during respawn.
    public static RespawnManager Instance { get; private set; } // Singleton instance of the RespawnManager.

    private int _respawnTime = 10; // Time (in seconds) for the respawn countdown.
    private int _health = 100; // Initial health value for respawned players.
    private Coroutine _countdownCoroutine; // Reference to the active countdown coroutine.

    /// <summary>
    /// Ensures only one instance of RespawnManager exists and persists across scenes.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initiates the respawn process for a player, including disabling controls and starting the countdown.
    /// </summary>
    /// <param name="player">The player GameObject to respawn.</param>
    public void StartRespawnPlayer(GameObject player)
    {
        if (IsServer)
        {
            NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
            if (playerNetworkObject != null)
            {
                ulong clientId = playerNetworkObject.OwnerClientId;
                ShowRespawnUIClientRpc(clientId);
                TurnOffPlayerControlsClientRpc(clientId);
                player.layer = _playerDeadLayer;
                player.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
    }

    /// <summary>
    /// Disables player controls for the specified client.
    /// </summary>
    /// <param name="clientId">The ID of the client whose controls should be disabled.</param>
    [ClientRpc]
    private void TurnOffPlayerControlsClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            NetworkObject localPlayerNetworkObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (localPlayerNetworkObject != null &&
                localPlayerNetworkObject.TryGetComponent<PlayerManager>(out var playerManager))
            {
                playerManager.inputReader.TurnOffPlayerControls();
            }
        }
    }

    /// <summary>
    /// Displays the respawn UI and starts the countdown for the specified client.
    /// </summary>
    /// <param name="clientId">The ID of the client to show the respawn UI for.</param>
    [ClientRpc]
    private void ShowRespawnUIClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (_respawnPlayerPanel != null && _countdownTimeText != null)
            {
                _respawnPlayerPanel.SetActive(true);
                _countdownCoroutine = StartCoroutine(RespawnCountdownCoroutine());
            }
        }
    }

    /// <summary>
    /// Handles the respawn countdown and triggers player respawn when the countdown ends.
    /// </summary>
    private IEnumerator RespawnCountdownCoroutine()
    {
        int currentTime = _respawnTime;

        while (currentTime > 0)
        {
            _countdownTimeText.text = currentTime.ToString();
            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        _countdownTimeText.text = "0";
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        DespawnPlayerServerRpc(clientId);
        RequestSpawnPlayerServerRpc(clientId);
        if (_respawnPlayerPanel != null)
        {
            _respawnPlayerPanel.SetActive(false);
        }

        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }
    }

    /// <summary>
    /// Despawns the player object for the specified client.
    /// </summary>
    /// <param name="clientId">The ID of the client whose player object should be despawned.</param>
    [ServerRpc(RequireOwnership = false)]
    private void DespawnPlayerServerRpc(ulong clientId)
    {
        NetworkObject playerNetworkObjectToRemove =
            NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        if (playerNetworkObjectToRemove.TryGetComponent<PlayerHealth>(out var playerHealth))
        {
            playerHealth.InitiateHealthBarDestructionClientRpc();
        }

        playerNetworkObjectToRemove.Despawn();
    }

    /// <summary>
    /// Spawns a new player object for the specified client and reinitializes its state.
    /// </summary>
    /// <param name="clientId">The ID of the client to spawn a new player for.</param>
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnPlayerServerRpc(ulong clientId)
    {
        NetworkPrefabsList networkPrefabsList = NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists[0];
        NetworkPrefab networkPrefabEntry =
            networkPrefabsList.PrefabList.FirstOrDefault(prefab =>
                prefab.Prefab.name == "Player");
        GameObject playerInstance =
            Instantiate(networkPrefabEntry.Prefab, Vector3.zero,
                Quaternion.identity);
        NetworkObject playerNetworkObject = playerInstance.GetComponent<NetworkObject>();

        if (playerNetworkObject != null)
        {
            playerNetworkObject.SpawnAsPlayerObject(clientId, true);
            playerInstance.GetComponent<PlayerManager>().inputReader.TurnOnPlayerControls();
            playerInstance.GetComponent<PlayerHealth>().InitializePlayerHealth(_health);
        }
    }
}