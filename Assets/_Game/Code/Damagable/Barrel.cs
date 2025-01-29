using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Barrel : NetworkBehaviour, IDamagable
{
    [SerializeField] private DamagableSettings _destructableSettings;

    private readonly NetworkVariable<float> _health = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _health.Value = _destructableSettings.health;
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
        if (_health.Value <= 0)
        {
            RpcShowBarrelDestroyForEveryOneClientRpc();
        }
        else
        {
            RpcShowBarrelDamageForEveryoneClientRpc();
        }
    }
}