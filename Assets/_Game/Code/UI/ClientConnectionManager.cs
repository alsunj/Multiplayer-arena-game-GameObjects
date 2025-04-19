using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ClientConnectionManager : MonoBehaviour
{
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


    private ushort Port => ushort.Parse(_portField.text);
    private int PlayerAmount => int.Parse(_playerAmountField.text);
    private int RogueEnemyAmount => int.Parse(_RogueEnemyAmountField.text);
    private int SlimeEnemyAmount => int.Parse(_SlimeEnemyAmountField.text);
    private string Address => _addressField.text;

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


    private async void OnButtonConnect()
    {
        GameObject networkManagerObject = NetworkManager.Singleton.gameObject;

        // AsyncOperation loadOperation = SceneManager.LoadSceneAsync("SC", LoadSceneMode.Additive);
        // while (!loadOperation.isDone)
        // {
        //     await System.Threading.Tasks.Task.Yield();
        // }
        //
        // Scene gameScene = SceneManager.GetSceneByName("SC");
        //
        // SceneManager.MoveGameObjectToScene(networkManagerObject, gameScene);
        //
        // SceneManager.SetActiveScene(gameScene);
        //
        // SceneManager.UnloadSceneAsync("ConnectionScene");

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

        GameObject gameConfigHolder = new GameObject("GameConfigHolder");
        GameDataConfig configComponent = gameConfigHolder.AddComponent<GameDataConfig>();

        configComponent.RogueEnemyAmount = RogueEnemyAmount;
        configComponent.SlimeEnemyAmount = SlimeEnemyAmount;

        //   SceneManager.MoveGameObjectToScene(gameConfigHolder, targetScene);
    }

    private void StartServer()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(Address, Port);
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        else
        {
            Debug.LogError("Unity Transport component not found on the NetworkManager!");
        }
    }

    private void OnClientConnected(ulong obj)
    {
        GameStartConfig.Instance.IncrementConnectedPlayers();
        Debug.Log(GameStartConfig.Instance.GetConnectedPlayers() + "amount connected");
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

    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
}