using UnityEngine;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartUIControllerGO : MonoBehaviour
{
    [SerializeField] private GameObject _beginGamePanel;
    [SerializeField] private GameObject _confirmQuitPanel;
    [SerializeField] private GameObject _countdownPanel;

    [SerializeField] private Button _quitWaitingButton;
    [SerializeField] private Button _confirmQuitButton;
    [SerializeField] private Button _cancelQuitButton;

    [SerializeField] private TMP_Text _waitingText;
    [SerializeField] private TMP_Text _countdownText;

    private NetworkManager _networkManager;

    private void OnEnable()
    {
        _beginGamePanel.SetActive(true);
        _confirmQuitPanel.SetActive(false);
        _countdownPanel.SetActive(false);

        _quitWaitingButton.onClick.AddListener(AttemptQuitWaiting);
        _confirmQuitButton.onClick.AddListener(ConfirmQuit);
        _cancelQuitButton.onClick.AddListener(CancelQuit);

        _networkManager = NetworkManager.Singleton;
        if (_networkManager != null)
        {
            // Subscribe to a custom event or check NetworkManager state directly
            // We'll assume you have a way to track players remaining on the server
            // and update clients via RPC or NetworkVariable.

            // Example: Subscribe to an event that the server triggers
            // if (NetworkGameManager.Instance != null)
            // {
            //     NetworkGameManager.Instance.OnPlayersRemainingToStartUpdated += UpdatePlayerRemainingText;
            //     NetworkGameManager.Instance.OnStartGameCountdown += BeginCountdown;
            // }
            //
            // // Example: If countdown is a NetworkVariable on a NetworkBehaviour
            // if (NetworkGameManager.Instance != null)
            // {
            //     NetworkGameManager.Instance.CountdownTime.OnValueChanged += UpdateCountdownTextFromNetworkVariable;
            //     if (NetworkGameManager.Instance.IsCountdownActive.Value)
            //     {
            //         BeginCountdown();
            //     }
            // }
        }
        else
        {
            Debug.LogError("NetworkManager not found!");
        }
    }

    private void OnDisable()
    {
        _quitWaitingButton.onClick.RemoveAllListeners();
        _confirmQuitButton.onClick.RemoveAllListeners();
        _cancelQuitButton.onClick.RemoveAllListeners();
        //
        // if (NetworkGameManager.Instance != null)
        // {
        //     NetworkGameManager.Instance.OnPlayersRemainingToStartUpdated -= UpdatePlayerRemainingText;
        //     NetworkGameManager.Instance.OnStartGameCountdown -= BeginCountdown;
        //     NetworkGameManager.Instance.CountdownTime.OnValueChanged -= UpdateCountdownTextFromNetworkVariable;
        // }
    }

    private void UpdatePlayerRemainingText(int playersRemainingToStart)
    {
        var playersText = playersRemainingToStart == 1 ? "player" : "players";
        _waitingText.text = $"Waiting for {playersRemainingToStart.ToString()} more {playersText} to join...";
    }

    private void UpdateCountdownText(int countdownTime)
    {
        _countdownText.text = countdownTime.ToString();
    }

    private void UpdateCountdownTextFromNetworkVariable(int previousValue, int newValue)
    {
        _countdownText.text = newValue.ToString();
    }

    private void AttemptQuitWaiting()
    {
        _beginGamePanel.SetActive(false);
        _confirmQuitPanel.SetActive(true);
    }

    private void ConfirmQuit()
    {
        StartCoroutine(DisconnectDelay());
    }

    IEnumerator DisconnectDelay()
    {
        yield return new WaitForSeconds(1f);
        if (_networkManager != null && _networkManager.IsClient)
        {
            _networkManager.Shutdown();
            SceneManager.LoadScene(0); // Load your main menu scene
        }
        else if (_networkManager != null && _networkManager.IsServer)
        {
            _networkManager.Shutdown();
            SceneManager.LoadScene(0); // Load your main menu scene
        }
        else
        {
            SceneManager.LoadScene(0); // If no network is active, just go to main menu
        }
    }

    private void CancelQuit()
    {
        _confirmQuitPanel.SetActive(false);
        _beginGamePanel.SetActive(true);
    }

    private void BeginCountdown()
    {
        _beginGamePanel.SetActive(false);
        _confirmQuitPanel.SetActive(false);
        _countdownPanel.SetActive(true);
    }

    private void EndCountdown()
    {
        _countdownPanel.SetActive(false);
    }
}