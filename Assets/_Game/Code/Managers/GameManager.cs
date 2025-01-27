using TMPro;
using Unity.Netcode;
using UnityEngine;


public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private bool startGame;
    private TextMeshProUGUI _timerText;
    private float _initialTimer = 10f;
    private float _startGameTimer = 10f;
    private bool _initialTimerActive = true;
    private bool _startGameTimerActive;


    private void Start()
    {
        _timerText = GameObject.Find("Start_Game").GetComponent<TextMeshProUGUI>();
        if (_timerText == null)
        {
            Debug.LogError("TextMeshPro component not found on Start Game GameObject!");
            return;
        }

        if (IsServer)
        {
            _initialTimerActive = startGame;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (_initialTimerActive)
        {
            _initialTimer -= Time.deltaTime;
            if (_initialTimer <= 0)
            {
                _initialTimerActive = false;
                _startGameTimerActive = true;
                EnableTimerTextClientRpc(true);
            }
        }

        if (_startGameTimerActive)
        {
            _startGameTimer -= Time.deltaTime;
            if (_startGameTimer > 0)
            {
                UpdateTimerClientRpc(_startGameTimer);
            }
            else
            {
                _startGameTimerActive = false;
                UpdateTimerClientRpc(0);
                EnableTimerTextClientRpc(false);
                TimersEndedServerRpc();
            }
        }
    }

    [ClientRpc]
    private void UpdateTimerClientRpc(float timeRemaining)
    {
        if (_timerText != null)
        {
            _timerText.text = $"Starting Game in {Mathf.Ceil(timeRemaining)}";
        }
    }

    [ClientRpc]
    private void EnableTimerTextClientRpc(bool state)
    {
        if (_timerText != null)
        {
            _timerText.enabled = state;
        }
    }

    [ServerRpc]
    private void TimersEndedServerRpc()
    {
        for (int i = 0; i < 2; i++)
        {
            Vector3 spawnPosition = new Vector3(10 + UnityEngine.Random.Range(-5f, 5f), 0,
                10 + Random.Range(-5f, 5f));
            GameObject obj = Instantiate(keyPrefab, spawnPosition, Quaternion.identity);
            obj.GetComponent<NetworkObject>().Spawn();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestTimerStateServerRpc(ServerRpcParams rpcParams = default)
    {
        if (_initialTimerActive)
        {
            RespondTimerStateClientRpc(_initialTimer, true, rpcParams.Receive.SenderClientId);
        }
        else if (_startGameTimerActive)
        {
            RespondTimerStateClientRpc(_startGameTimer, true, rpcParams.Receive.SenderClientId);
        }
        else
        {
            RespondTimerStateClientRpc(0, false, rpcParams.Receive.SenderClientId);
        }
    }

    [ClientRpc]
    private void RespondTimerStateClientRpc(float timeRemaining, bool timerActive, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            _timerText.enabled = timerActive;
            _timerText.text = $"Starting Game in {Mathf.Ceil(timeRemaining)}";
        }
    }
}