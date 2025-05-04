using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RespawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject _respawnPlayerPanel;
    [SerializeField] private TextMeshProUGUI _countdownTimeText;
    [SerializeField] private int _playerDeadLayer = 11;
    public static RespawnManager Instance { get; private set; }

    private int _respawnTime = 10;
    private int _health = 100;
    private Coroutine _countdownCoroutine;


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