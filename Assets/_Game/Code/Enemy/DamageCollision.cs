using Unity.Netcode;
using UnityEngine;

public class DamageCollision : NetworkBehaviour
{
    [SerializeField] private DamageCollisionSettings _damageCollisionSettings;
    private int _targetLayer;
    private int _terrainLayer;
    private int _damage;

    public override void OnNetworkSpawn()
    {
        _targetLayer = GetLayerFromLayerMask(_damageCollisionSettings.targetLayer);
        _terrainLayer = GetLayerFromLayerMask(_damageCollisionSettings.terrainLayer);
        _damage = _damageCollisionSettings.damageAmount;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (IsServer)
        {
            if (other.gameObject.layer == _targetLayer)
            {
                other.gameObject.GetComponent<PlayerHealth>().decreaseHealth(_damage);

                if (gameObject.TryGetComponent(out Slime enemyComponent))
                {
                    TargetingManager.Instance.RemoveEnemyFromTargetingList(enemyComponent);
                    gameObject.GetComponent<NetworkObject>().Despawn();
                    Destroy(this);
                }

                if (gameObject.TryGetComponent(out Arrow arrowComponent))
                {
                    NetworkObjectReference arrowNetworkObjectReference =
                        arrowComponent.GetComponent<NetworkObject>();
                    DisableArrowClientRpc(arrowNetworkObjectReference);
                }
            }

            if (other.gameObject.layer == _terrainLayer)
            {
                if (gameObject.TryGetComponent(out Arrow arrowComponent))
                {
                    NetworkObjectReference arrowNetworkObjectReference =
                        arrowComponent.GetComponent<NetworkObject>();
                    DisableArrowClientRpc(arrowNetworkObjectReference);
                }
            }
        }
    }

    [ClientRpc]
    private void DisableArrowClientRpc(NetworkObjectReference arrowReference)
    {
        if (arrowReference.TryGet(out NetworkObject arrowToEnable))
        {
            arrowToEnable.gameObject.SetActive(false);
        }
    }

    public static int GetLayerFromLayerMask(LayerMask layerMask)
    {
        int value = layerMask.value;
        if (value == 0) return -1;

        int layer = 0;
        while (value > 1)
        {
            value >>= 1;
            layer++;
        }

        return layer;
    }
}