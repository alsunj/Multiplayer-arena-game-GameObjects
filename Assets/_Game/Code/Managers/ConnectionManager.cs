using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the connection process for a multiplayer game, including server and client setup,
/// player spawning, and game start countdown.
/// </summary>
public class ConnectionManager : NetworkBehaviour
{
    #region UIGameObjectReferences

    /// <summary>
    /// Panel displayed while waiting for players to join.
    /// </summary>
    [SerializeField] private GameObject _WaitingPlayerPanel;

    /// <summary>
    /// Text displaying the number of players needed to start.
    /// </summary>
    [SerializeField] private TextMeshProUGUI _waitingText;

    /// <summary>
    /// Panel displayed during the game start countdown.
    /// </summary>
    [SerializeField] private GameObject _countdownPanel;

    /// <summary>
    /// Text displaying the countdown timer.
    /// </summary>
    [SerializeField] private TextMeshProUGUI _countdownText;

    /// <summary>
    /// Panel for connection settings.
    /// </summary>
    [SerializeField] private GameObject _connectionPanel;

    /// <summary>
    /// Input field for the server address.
    /// </summary>
    [SerializeField] private TMP_InputField _addressField;

    /// <summary>
    /// Input field for the server port.
    /// </summary>
    [SerializeField] private TMP_InputField _portField;

    /// <summary>
    /// Dropdown to select connection mode (host/client).
    /// </summary>
    [SerializeField] private TMP_Dropdown _connectionModeDropdown;

    /// <summary>
    /// Input field for the number of players.
    /// </summary>
    [SerializeField] private TMP_InputField _playerAmountField;

    /// <summary>
    /// Input field for the number of Rogue enemies.
    /// </summary>
    [SerializeField] private TMP_InputField _RogueEnemyAmountField;

    /// <summary>
    /// Input field for the number of Slime enemies.
    /// </summary>
    [SerializeField] private TMP_InputField _SlimeEnemyAmountField;

    /// <summary>
    /// UI container for lobby-related settings.
    /// </summary>
    [SerializeField] private GameObject LobbyAmountContainer;

    /// <summary>
    /// UI container for Ranger enemy settings.
    /// </summary>
    [SerializeField] private GameObject RangerAmountContainer;

    /// <summary>
    /// UI container for Slime enemy settings.
    /// </summary>
    [SerializeField] private GameObject SlimeAmountContainer;

    /// <summary>
    /// Button to initiate the connection process.
    /// </summary>
    [SerializeField] private Button _connectButton;

    /// <summary>
    /// Cooldown time for spawning Slime enemies.
    /// </summary>
    [SerializeField] private float _slimeSpawnCooldownTime;

    /// <summary>
    /// Cooldown time for spawning Rogue enemies.
    /// </summary>
    [SerializeField] private float _rogueSpawnCooldownTime;

    /// <summary>
    /// Default countdown timer value.
    /// </summary>
    [SerializeField] private int _startGameCountdownTimer = 10;

    /// <summary>
    /// Prefab for player objects.
    /// </summary>
    [SerializeField] private GameObject playerPrefab;

    /// <summary>
    /// Initial health for players.
    /// </summary>
    [SerializeField] private int playerHealth;

    /// <summary>
    /// Temporary object to hold game configuration data.
    /// </summary>
    private GameObject gameConfigHolder;

    /// <summary>
    /// List of connected client IDs.
    /// </summary>
    private List<ulong> connectedClientIds = new List<ulong>();

    /// <summary>
    /// Port number parsed from the input field.
    /// </summary>
    private ushort Port => ushort.Parse(_portField.text);

    /// <summary>
    /// Number of players parsed from the input field.
    /// </summary>
    private int PlayerAmount => int.Parse(_playerAmountField.text);

    /// <summary>
    /// Number of Rogue enemies parsed from the input field.
    /// </summary>
    private int RogueEnemyAmount => int.Parse(_RogueEnemyAmountField.text);

    /// <summary>
    /// Number of Slime enemies parsed from the input field.
    /// </summary>
    private int SlimeEnemyAmount => int.Parse(_SlimeEnemyAmountField.text);

    /// <summary>
    /// Server address parsed from the input field.
    /// </summary>
    private string Address => _addressField.text;

    /// <summary>
    /// Indicates whether the game start timer is running.
    /// </summary>
    private bool _timerRunning = false;

    /// <summary>
    /// Countdown timer value on the server.
    /// </summary>
    private int _serverCountdown;

    #endregion

    /// <summary>
    /// Subscribes to UI events when the object is enabled.
    /// </summary>
    private void OnEnable()
    {
        _connectionModeDropdown.onValueChanged.AddListener(OnConnectionModeChanged);
        _connectButton.onClick.AddListener(OnButtonConnect);
        OnConnectionModeChanged(_connectionModeDropdown.value);
    }

    /// <summary>
    /// Unsubscribes from UI events when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        _connectionModeDropdown.onValueChanged.RemoveAllListeners();
        _connectButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Updates the UI based on the selected connection mode (host or client).
    /// </summary>
    /// <param name="connectionMode">The selected connection mode.</param>
    private void OnConnectionModeChanged(int connectionMode)
    {
        string buttonLabel;
        _connectButton.enabled = true;

        switch (connectionMode)
        {
            case 0:
                buttonLabel = "Start Host";
                LobbyAmountContainer.SetActive(true);
                RangerAmountContainer.SetActive(true);
                SlimeAmountContainer.SetActive(true);
                break;
            case 1:
                buttonLabel = "Start Client";
                LobbyAmountContainer.SetActive(false);
                RangerAmountContainer.SetActive(false);
                SlimeAmountContainer.SetActive(false);
                break;
            default:
                buttonLabel = "<ERROR>";
                _connectButton.enabled = false;
                break;
        }

        var buttonText = _connectButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = buttonLabel;
    }

    /// <summary>
    /// Handles the connection button click event and starts the host or client based on the selected mode.
    /// </summary>
    private void OnButtonConnect()
    {
        _connectionPanel.SetActive(false);
        _WaitingPlayerPanel.SetActive(true);
        switch (_connectionModeDropdown.value)
        {
            case 0:
                StartServer();
                SetupGameConfig();
                break;
            case 1:
                StartClient();
                break;
            default:
                Debug.LogError("Error: Unknown connection mode", gameObject);
                break;
        }
    }

    /// <summary>
    /// Sets up the game configuration by creating a temporary object to hold enemy amounts.
    /// </summary>
    private void SetupGameConfig()
    {
        gameConfigHolder = new GameObject("GameConfigHolder");
        GameDataConfig configComponent = gameConfigHolder.AddComponent<GameDataConfig>();
        configComponent.RogueEnemyAmount = RogueEnemyAmount;
        configComponent.SlimeEnemyAmount = SlimeEnemyAmount;
        configComponent.SlimeEnemySpawnTimer = _slimeSpawnCooldownTime;
        configComponent.RogueEnemySpawnTimer = _rogueSpawnCooldownTime;
    }

    /// <summary>
    /// Starts the server and sets up the host connection.
    /// </summary>
    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(Address, Port);
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.OnConnectionEvent += OnNetworkConnectionEvent;
        connectedClientIds.Add(NetworkManager.Singleton.LocalClientId);
        CheckConnectedClientAmount();
    }

    /// <summary>
    /// Handles network connection events such as client connections and disconnections.
    /// </summary>
    /// <param name="networkManager">The NetworkManager instance.</param>
    /// <param name="connectionEvent">The connection event data.</param>
    private void OnNetworkConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEvent)
    {
        if (IsServer)
        {
            if (connectionEvent.EventType == ConnectionEvent.ClientConnected)
            {
                Debug.Log($"Client connected with ID: {connectionEvent.ClientId}");
                connectedClientIds.Add(connectionEvent.ClientId);
                Debug.Log($"Connected Client Count: {connectedClientIds.Count}");
            }
            else if (connectionEvent.EventType == ConnectionEvent.ClientDisconnected)
            {
                Debug.Log($"Client disconnected with ID: {connectionEvent.ClientId}");
                connectedClientIds.Remove(connectionEvent.ClientId);
                Debug.Log($"Connected Client Count: {connectedClientIds.Count}");
            }

            CheckConnectedClientAmount();
        }
    }

    /// <summary>
    /// Checks the number of connected clients and starts the game if the required number is met.
    /// </summary>
    private void CheckConnectedClientAmount()
    {
        if (connectedClientIds.Count >= PlayerAmount && !_timerRunning)
        {
            BeginGameStart();
        }
        else
        {
            UpdatePlayerRemainingTextClientRpc(PlayerAmount - connectedClientIds.Count);
        }
    }

    /// <summary>
    /// Begins the game start countdown and notifies clients.
    /// </summary>
    private void BeginGameStart()
    {
        _serverCountdown = _startGameCountdownTimer;
        StartCoroutine(StartGameTimer());
        BeginGameStartClientRpc(_serverCountdown);
    }

    /// <summary>
    /// Starts the game countdown timer on the server.
    /// </summary>
    private IEnumerator StartGameTimer()
    {
        _timerRunning = true;
        OpenCountDownPanelClientRpc();
        PrepareGameLoadSceneForServerAsync();
        while (_serverCountdown > 0)
        {
            yield return new WaitForSeconds(1f);
            _serverCountdown--;
            UpdateCountdownClientRpc(_serverCountdown);
        }

        UnloadSceneClientRpc("ConnectionScene");
        SpawnPlayers();
        EnemySpawnerManager.Instance.StartEnemySpawnTimers();
        gameObject.GetComponent<NetworkObject>().Despawn();
    }

    /// <summary>
    /// Unloads the specified scene on all clients.
    /// </summary>
    /// <param name="sceneName">The name of the scene to unload.</param>
    [ClientRpc]
    private void UnloadSceneClientRpc(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    /// <summary>
    /// Closes the game start panel.
    /// </summary>
    private void CloseGameStartPanel()
    {
        _countdownPanel.SetActive(false);
    }

    /// <summary>
    /// Prepares the server to load the game scene asynchronously.
    /// </summary>
    private void PrepareGameLoadSceneForServerAsync()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("SC", LoadSceneMode.Additive);
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadCompleted;
    }

    /// <summary>
    /// Handles the completion of the scene load event.
    /// </summary>
    /// <param name="scenename">The name of the loaded scene.</param>
    /// <param name="loadscenemode">The mode in which the scene was loaded.</param>
    /// <param name="clientscompleted">List of clients that completed the load.</param>
    /// <param name="clientstimedout">List of clients that timed out during the load.</param>
    private void OnSceneLoadCompleted(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted,
        List<ulong> clientstimedout)
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
        Scene gameScene = SceneManager.GetSceneByName(scenename);
        SceneManager.MoveGameObjectToScene(gameConfigHolder, gameScene);
        CheckSceneLoadCompletion(connectedClientIds, clientscompleted, clientstimedout);
        MoveNetworkManagerToNewSceneClientRpc(scenename);
    }

    /// <summary>
    /// Moves the NetworkManager to the new scene on all clients.
    /// </summary>
    /// <param name="scenename">The name of the new scene.</param>
    [ClientRpc]
    private void MoveNetworkManagerToNewSceneClientRpc(string scenename)
    {
        Scene scene = SceneManager.GetSceneByName(scenename);
        SceneManager.MoveGameObjectToScene(NetworkManager.Singleton.gameObject, scene);
        SceneManager.SetActiveScene(scene);
    }

    /// <summary>
    /// Checks the completion status of the scene load for all clients.
    /// </summary>
    /// <param name="initialConnectedClientIds">List of initially connected client IDs.</param>
    /// <param name="clientsCompleted">List of clients that completed the load.</param>
    /// <param name="clientsTimedOut">List of clients that timed out during the load.</param>
    private void CheckSceneLoadCompletion(List<ulong> initialConnectedClientIds, List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in initialConnectedClientIds)
        {
            if (clientsCompleted.Contains(clientId))
            {
                Debug.Log($"[Server] Client ID {clientId} successfully loaded the scene.");
            }
            else if (clientsTimedOut.Contains(clientId))
            {
                Debug.LogWarning($"[Server] Client ID {clientId} timed out during scene load.");
            }
            else
            {
                Debug.LogWarning(
                    $"[Server] Client ID {clientId} from initial connection list neither completed nor timed out during scene load. This might indicate a disconnection or other issue.");
            }
        }
    }

    /// <summary>
    /// Notifies clients to begin the game start countdown.
    /// </summary>
    /// <param name="initialCountdownValue">The initial countdown value.</param>
    [ClientRpc]
    private void BeginGameStartClientRpc(int initialCountdownValue)
    {
        _WaitingPlayerPanel.SetActive(false);
        _countdownPanel.SetActive(true);
        _countdownText.text = initialCountdownValue.ToString();
    }

    /// <summary>
    /// Updates the remaining player count text on all clients.
    /// </summary>
    /// <param name="remainingPlayers">The number of players still needed to start the game.</param>
    [ClientRpc]
    private void UpdatePlayerRemainingTextClientRpc(int remainingPlayers)
    {
        var playersText = remainingPlayers == 1 ? "player" : "players";
        _waitingText.text = $"Waiting for {remainingPlayers} more {playersText} to join...";
    }

    /// <summary>
    /// Updates the countdown timer text on all clients.
    /// </summary>
    /// <param name="currentCountdownValue">The current countdown value.</param>
    [ClientRpc]
    private void UpdateCountdownClientRpc(int currentCountdownValue)
    {
        _countdownText.text = currentCountdownValue.ToString();
    }

    /// <summary>
    /// Opens the countdown panel on all clients.
    /// </summary>
    [ClientRpc]
    private void OpenCountDownPanelClientRpc()
    {
        CloseGameStartPanel();
    }

    /// <summary>
    /// Spawns player objects for all connected clients.
    /// </summary>
    private void SpawnPlayers()
    {
        foreach (ulong clientId in connectedClientIds)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
            Quaternion spawnRotation = Quaternion.identity;

            GameObject spawnedPlayer = Instantiate(playerPrefab, spawnPosition, spawnRotation);
            NetworkObject networkObject = spawnedPlayer.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.SpawnAsPlayerObject(clientId, true);
                spawnedPlayer.GetComponent<PlayerHealth>().InitializePlayerHealth(playerHealth);
            }
            else
            {
                Destroy(spawnedPlayer);
                Debug.LogError($"[Server] Failed to get NetworkObject on spawned player for client {clientId}.");
            }
        }
    }

    /// <summary>
    /// Starts the client and connects to the server.
    /// </summary>
    private void StartClient()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(Address, Port);
            NetworkManager.Singleton.StartClient();
        }
        else
        {
            Debug.LogError("Unity Transport component not found on the NetworkManager!");
        }
    }
}