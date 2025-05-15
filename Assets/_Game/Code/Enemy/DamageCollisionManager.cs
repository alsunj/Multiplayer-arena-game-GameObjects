using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages collision-based damage interactions in a networked environment.
/// This class handles damage application, object despawning, and client-side updates
/// for projectiles and enemies upon collision with targets or terrain.
/// </summary>
public class DamageCollisionManager : NetworkBehaviour
{
    /// <summary>
    /// Settings for damage collision, including target and terrain layers and damage amount.
    /// </summary>
    [SerializeField] private DamageCollisionSettings _damageCollisionSettings;

    private int _targetLayer; // Layer for target objects (e.g., players).
    private int _terrainLayer; // Layer for terrain objects.
    private int _damage; // Amount of damage to apply to targets.

    /// <summary>
    /// Called when the object is spawned on the network.
    /// Initializes layer and damage values from the provided settings.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        _targetLayer = GetLayerFromLayerMask(_damageCollisionSettings.targetLayer);
        _terrainLayer = GetLayerFromLayerMask(_damageCollisionSettings.terrainLayer);
        _damage = _damageCollisionSettings.damageAmount;
    }

    /// <summary>
    /// Handles collision events and applies appropriate logic based on the collided object's layer.
    /// </summary>
    /// <param name="other">The collision data of the object this collided with.</param>
    private void OnCollisionEnter(Collision other)
    {
        if (IsServer) // Ensure logic is executed only on the server.
        {
            if (other.gameObject.layer == _targetLayer)
            {
                // Apply damage to the target if it has a PlayerHealth component.
                other.gameObject.GetComponent<PlayerHealth>().decreaseHealth(_damage);

                // Handle enemy despawning and removal from targeting list.
                if (gameObject.TryGetComponent(out Slime enemyComponent))
                {
                    TargetingManager.Instance.RemoveEnemyFromTargetingList(enemyComponent);
                    gameObject.GetComponent<NetworkObject>().Despawn();
                }

                // Handle arrow-specific logic for disabling the arrow on clients.
                if (gameObject.TryGetComponent(out Arrow arrowComponent))
                {
                    NetworkObjectReference arrowNetworkObjectReference =
                        arrowComponent.GetComponent<NetworkObject>();
                    DisableArrowClientRpc(arrowNetworkObjectReference);
                }
            }

            if (other.gameObject.layer == _terrainLayer)
            {
                // Handle arrow-specific logic for disabling the arrow on clients when hitting terrain.
                if (gameObject.TryGetComponent(out Arrow arrowComponent))
                {
                    NetworkObjectReference arrowNetworkObjectReference =
                        arrowComponent.GetComponent<NetworkObject>();
                    DisableArrowClientRpc(arrowNetworkObjectReference);
                }
            }
        }
    }

    /// <summary>
    /// Disables the arrow object on all clients.
    /// </summary>
    /// <param name="arrowReference">A reference to the arrow's NetworkObject.</param>
    [ClientRpc]
    private void DisableArrowClientRpc(NetworkObjectReference arrowReference)
    {
        if (arrowReference.TryGet(out NetworkObject arrowToEnable))
        {
            arrowToEnable.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Converts a LayerMask to its corresponding layer index.
    /// </summary>
    /// <param name="layerMask">The LayerMask to convert.</param>
    /// <returns>The index of the layer, or -1 if the LayerMask is invalid.</returns>
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