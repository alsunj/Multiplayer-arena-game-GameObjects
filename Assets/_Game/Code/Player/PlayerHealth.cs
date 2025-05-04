using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private GameObject _healthCanvas;
    private Slider _healthSlider;
    private NetworkVariable<int> playerHealth = new NetworkVariable<int>();
    private Vector3 _healthBarOffset;
    private bool isRespawning;

    public override void OnNetworkSpawn()
    {
        _healthBarOffset = _healthCanvas.transform.localPosition;
        _healthCanvas.transform.SetParent(null);
        _healthSlider = _healthCanvas.GetComponentInChildren<Slider>();
    }

    [ClientRpc]
    private void StartHealthBarClientRpc(int health)
    {
        _healthSlider.maxValue = health;
    }


    public void InitializePlayerHealth(int health)
    {
        if (IsServer)
        {
            isRespawning = false;
            playerHealth.Value = health;
            StartHealthBarClientRpc(playerHealth.Value);
        }
    }

    private void Update()
    {
        _healthCanvas.transform.position = gameObject.transform.position + _healthBarOffset;
        _healthSlider.value = playerHealth.Value;
    }

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

    // [ClientRpc]
    // private void UpdateHealthClientRpc(int curHealth)
    // {
    //     _healthSlider.value = curHealth;
    // }

    // private void OnDestroy()
    // {
    //     if (IsServer)
    //     {
    //         Destroy(_healthCanvas);
    //
    //         DestroyHealthBarClientRpc();
    //     }
    // }
    //
    // [ClientRpc]
    // private void DestroyHealthBarClientRpc()
    // {
    //     if (_healthCanvas != null)
    //     {
    //         Destroy(_healthCanvas);
    //     }
    //     else
    //     {
    //         Debug.LogError("Cant find canvas");
    //     }
    // }
    [ClientRpc]
    public void InitiateHealthBarDestructionClientRpc()
    {
        Destroy(_healthCanvas);
    }
}