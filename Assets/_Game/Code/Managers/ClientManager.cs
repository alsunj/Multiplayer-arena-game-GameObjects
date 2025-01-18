using Unity.Netcode;
using UnityEngine;

public class ClientManager : NetworkBehaviour
{
    private void Start()
    {
        if (IsClient && !IsServer)
        {
            RequestTimerState();
        }
    }

    private void RequestTimerState()
    {
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.RequestTimerStateServerRpc();
            }
            else
            {
                Debug.LogError("GameManager not found.");
            }
        }
    }
}