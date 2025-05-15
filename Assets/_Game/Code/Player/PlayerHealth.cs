using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's health, including health bar updates and respawn logic.
/// </summary>
public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private GameObject _healthCanvas; // The canvas displaying the player's health bar.
    private Slider _healthSlider; // The slider component representing the health bar.

    private NetworkVariable<int>
        playerHealth = new NetworkVariable<int>(); // The player's health value synchronized across the network.

    private Vector3 _healthBarOffset; // Offset for positioning the health bar above the player.
    private bool isRespawning; // Tracks whether the player is currently respawning.

    /// <summary>
    /// Called when the object is spawned on the network.
    /// Initializes the health bar and its position.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        _healthBarOffset = _healthCanvas.transform.localPosition;
        _healthCanvas.transform.SetParent(null);
        _healthSlider = _healthCanvas.GetComponentInChildren<Slider>();
    }

    /// <summary>
    /// Initializes the player's health and starts the health bar.
    /// </summary>
    /// <param name="health">The initial health value.</param>
    public void InitializePlayerHealth(int health)
    {
        if (IsServer)
        {
            isRespawning = false;
            playerHealth.Value = health;
            StartHealthBarClientRpc(playerHealth.Value);
        }
    }

    /// <summary>
    /// Updates the health bar's position and value.
    /// </summary>
    private void Update()
    {
        _healthCanvas.transform.position = gameObject.transform.position + _healthBarOffset;
        _healthSlider.value = playerHealth.Value;
    }

    /// <summary>
    /// Decreases the player's health and triggers respawn if health reaches zero.
    /// </summary>
    /// <param name="health">The amount of health to decrease.</param>
    public void decreaseHealth(int health)
    {
        if (IsServer)
        {
            if (isRespawning) return;
            playerHealth.Value -= health;
            if (playerHealth.Value <= 0)
            {
                isRespawning = true;
                RespawnManager.Instance.StartRespawnPlayer(gameObject);
            }
        }
    }

    /// <summary>
    /// Starts the health bar on the client with the specified maximum health.
    /// </summary>
    /// <param name="health">The maximum health value.</param>
    [ClientRpc]
    private void StartHealthBarClientRpc(int health)
    {
        _healthSlider.maxValue = health;
    }

    /// <summary>
    /// Initiates the destruction of the health bar on all clients.
    /// </summary>
    [ClientRpc]
    public void InitiateHealthBarDestructionClientRpc()
    {
        Destroy(_healthCanvas);
    }
}