using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Barrel : NetworkBehaviour, IDestrucable
{
    [SerializeField] private DestructableSettings _destructableSettings;

    private readonly NetworkVariable<float> _health = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _health.Value = _destructableSettings.health;
        }

        _health.OnValueChanged += OnHealthChanged;
    }

    public override void OnDestroy()
    {
        _health.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        if (newHealth <= 0)
        {
            transform.DOScale(Vector3.zero, 0.5f).SetDelay(_destructableSettings.hitDelay)
                .OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            transform.DOShakePosition(0.5f, _destructableSettings.shakeOffset, 10, 90, false, true)
                .SetDelay(_destructableSettings.hitDelay);
        }
    }

    [ClientRpc]
    private void RpcShowBarrelDestroyForEveryOneClientRpc()
    {
        transform.DOScale(Vector3.zero, 0.5f).SetDelay(_destructableSettings.hitDelay)
            .OnComplete(() => gameObject.SetActive(false));
    }

    [ClientRpc]
    private void RpcShowBarrelDamageForEveryoneClientRpc()
    {
        transform.DOShakePosition(0.5f, _destructableSettings.shakeOffset, 10, 90, false, true)
            .SetDelay(_destructableSettings.hitDelay);
    }

    public void TakeDamage(float damage)
    {
        RequestTakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestTakeDamageServerRpc(float damage)
    {
        _health.Value -= damage;
        Debug.Log(_health.Value);
        Debug.Log("health value at checking: " + _health.Value);

        if (_health.Value <= 0)
        {
            CmdShowBarrelDestroyForEveryOneServerRpc();
        }
        else
        {
            CmdShowBarrelDamageForEveryoneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdShowBarrelDestroyForEveryOneServerRpc()
    {
        RpcShowBarrelDestroyForEveryOneClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdShowBarrelDamageForEveryoneServerRpc()
    {
        RpcShowBarrelDamageForEveryoneClientRpc();
    }
}