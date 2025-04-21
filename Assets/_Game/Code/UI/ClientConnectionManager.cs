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


public class ClientConnectionManager : NetworkBehaviour
{
    [SerializeField] private GameObject _WaitingPlayerPanel;
    [SerializeField] private TextMeshProUGUI _waitingText;

    //[SerializeField] private GameObject _confirmQuitPanel;
    [SerializeField] private GameObject _countdownPanel;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private GameObject _connectionPanel;
    [SerializeField] private TMP_InputField _addressField;
    [SerializeField] private TMP_InputField _portField;
    [SerializeField] private TMP_Dropdown _connectionModeDropdown;
    [SerializeField] private TMP_InputField _playerAmountField;
    [SerializeField] private TMP_InputField _RogueEnemyAmountField;
    [SerializeField] private TMP_InputField _SlimeEnemyAmountField;
    [SerializeField] private GameObject LobbyAmountContainer;
    [SerializeField] private GameObject RangerAmountContainer;
    [SerializeField] private GameObject SlimeAmountContainer;
    [SerializeField] private Button _connectButton;
    [SerializeField] private int _gameStartCountDownTime;
    [SerializeField] private float _slimeSpawnCooldownTime;
    [SerializeField] private float _rogueSpawnCooldownTime;
    [SerializeField] private GameObject playerPrefab; // Set the name in the Inspector

    [SerializeField] private int _startGameCountdownTimer = 10;

    private GameObject gameConfigHolder;

    private List<ulong> connectedClientIds = new List<ulong>(); // Use a List for dynamic size
    private ushort Port => ushort.Parse(_portField.text);
    private int PlayerAmount => int.Parse(_playerAmountField.text);
    private int RogueEnemyAmount => int.Parse(_RogueEnemyAmountField.text);
    private int SlimeEnemyAmount => int.Parse(_SlimeEnemyAmountField.text);
    private string Address => _addressField.text;

    private bool _timerRunning = false;
    private int _serverCountdown;

    private void OnEnable()
    {
        _connectionModeDropdown.onValueChanged.AddListener(OnConnectionModeChanged);
        _connectButton.onClick.AddListener(OnButtonConnect);
        OnConnectionModeChanged(_connectionModeDropdown.value);
    }

    private void OnDisable()
    {
        _connectionModeDropdown.onValueChanged.RemoveAllListeners();
        _connectButton.onClick.RemoveAllListeners();
    }

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


    private void SetupGameConfig()
    {
        GameObject cameStartConfigHolder = new GameObject("GameStartConfigHolder");
        GameStartConfig gameStartConfig = cameStartConfigHolder.AddComponent<GameStartConfig>();
        gameStartConfig.PlayerAmount = PlayerAmount;

        gameConfigHolder = new GameObject("GameConfigHolder");
        GameDataConfig configComponent = gameConfigHolder.AddComponent<GameDataConfig>();

        configComponent.RogueEnemyAmount = RogueEnemyAmount;
        configComponent.SlimeEnemyAmount = SlimeEnemyAmount;

        //   SceneManager.MoveGameObjectToScene(gameConfigHolder, targetScene);
    }

    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(Address, Port);
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.OnConnectionEvent += OnNetworkConnectionEvent;
        connectedClientIds.Add(NetworkManager.Singleton.LocalClientId);
        CheckConnectedClientAmount();
    }


    private void OnNetworkConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEvent)
    {
        if (IsServer)
        {
            if (connectionEvent.EventType == ConnectionEvent.ClientConnected)
            {
                Debug.Log($"Client connected with ID: {connectionEvent.ClientId}");
                connectedClientIds.Add(connectionEvent.ClientId);
                Debug.Log($"Connected Client Count: {connectedClientIds.Count}");
                // Update your UI here based on connectedClientIds.Count
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

    //must be executed only by the server
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

    //must be executed only by the server
    private void BeginGameStart()
    {
        _serverCountdown = _startGameCountdownTimer;
        StartCoroutine(StartGameTimer());
        BeginGameStartClientRpc(_serverCountdown); // Initial notification
    }

    //must be executed only by the server

    private IEnumerator StartGameTimer()
    {
        _timerRunning = true;
        OpenCountDownPanelClientRpc();
        PrepareGameLoadSceneForServerAsync();
        while (_serverCountdown > 0)
        {
            yield return new WaitForSeconds(1f);
            _serverCountdown--;
            UpdateCountdownClientRpc(_serverCountdown); // Update clients with the current value
        }

        UnloadSceneClientRpc("ConnectionScene");
        SpawnPlayers();
        Destroy(gameObject);
    }

    [ClientRpc]
    private void UnloadSceneClientRpc(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    private void CloseGameStartPanel()
    {
        _countdownPanel.SetActive(false);
    }

    private void PrepareGameLoadSceneForServerAsync()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("SC", LoadSceneMode.Additive);
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadCompleted;
    }

    private void OnSceneLoadCompleted(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted,
        List<ulong> clientstimedout)
    {
        if (IsServer)
        {
            Scene gameScene = SceneManager.GetSceneByName(scenename);
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
            SceneManager.MoveGameObjectToScene(NetworkManager.Singleton.gameObject, gameScene);
            SceneManager.MoveGameObjectToScene(gameConfigHolder, gameScene);
            SceneManager.SetActiveScene(gameScene);
            CheckSceneLoadCompletion(connectedClientIds, clientscompleted, clientstimedout);
        }
    }

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


    [ClientRpc]
    private void BeginGameStartClientRpc(int initialCountdownValue)
    {
        _WaitingPlayerPanel.SetActive(false);
        _countdownPanel.SetActive(true);
        _countdownText.text = initialCountdownValue.ToString();
    }


    [ClientRpc]
    private void UpdatePlayerRemainingTextClientRpc(int remainingPlayers)
    {
        var playersText = remainingPlayers == 1 ? "player" : "players";
        _waitingText.text = $"Waiting for {remainingPlayers} more {playersText} to join...";
    }

    [ClientRpc]
    private void UpdateCountdownClientRpc(int currentCountdownValue)
    {
        _countdownText.text = currentCountdownValue.ToString();
    }


    [ClientRpc]
    private void OpenCountDownPanelClientRpc()
    {
        CloseGameStartPanel();
    }

    private void SpawnPlayers()
    {
        foreach (ulong clientId in connectedClientIds)
        {
            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;

            GameObject spawnedPlayer = Instantiate(playerPrefab, spawnPosition, spawnRotation);
            NetworkObject networkObject = spawnedPlayer.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.SpawnAsPlayerObject(clientId, true);
            }
            else
            {
                Destroy(spawnedPlayer);
                Debug.LogError($"[Server] Failed to get NetworkObject on spawned player for client {clientId}.");
            }
        }
    }

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
// would be ideal to reject connections if the game has already started but unity does not have a way to do this
// one way would be rejecting the connections in the OnNetworkConnectionEvent method or setting strange values in server connection data