using System;
using Unity.Netcode;
using UnityEngine;

public abstract class Pickupable : NetworkBehaviour, IEquatable<Pickupable>
{
    private NetworkVariable<bool> isObjectPickedup = new NetworkVariable<bool>();
    private FollowTransform _followTransform;
    private Quaternion _startingRotation;

    private void Start()
    {
        _followTransform = GetComponent<FollowTransform>();
        if (_followTransform == null)
        {
            Debug.LogError("FollowTransform component not found on the GameObject.");
        }
    }

    public void RequestPutDownObject(Vector3 position)
    {
        RequestPutDownObjectServerRpc(position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPutDownObjectServerRpc(Vector3 position)
    {
        if (isObjectPickedup.Value)
        {
            isObjectPickedup.Value = false;
            _followTransform.SetTargetTransform(null);
            PutDownObjectClientRpc(position);
        }
    }

    [ClientRpc]
    private void PutDownObjectClientRpc(Vector3 position)
    {
        PutDown(position);
    }

    public void RequestPickupObject(NetworkObjectReference pickupingTarget)
    {
        RequestPickupObjectServerRpc(pickupingTarget);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupObjectServerRpc(NetworkObjectReference pickupingTarget)
    {
        if (!isObjectPickedup.Value)
        {
            isObjectPickedup.Value = true;

            if (pickupingTarget.TryGet(out NetworkObject target))
            {
                PlayerPlacements playerPlacements = target.GetComponent<PlayerPlacements>();
                if (playerPlacements != null)
                {
                    playerPlacements.SetPlayerRightHandItemServerRpc(new NetworkObjectReference(this.NetworkObject));
                    _followTransform.SetTargetTransform(playerPlacements.playerRightHand.transform);
                }
            }

            PickupObjectClientRpc(pickupingTarget);
        }
    }

    [ClientRpc]
    private void PickupObjectClientRpc(NetworkObjectReference pickupingTarget)
    {
        if (pickupingTarget.TryGet(out NetworkObject target))
        {
            Pickup(target.gameObject);
        }
    }

    public abstract void PutDown(Vector3 position);

    public abstract void Pickup(GameObject pickupingTarget);

    public bool Equals(Pickupable other)
    {
        if (other == null) return false;
        return NetworkObjectId == other.NetworkObjectId;
    }

    public override bool Equals(object obj)
    {
        if (obj is Pickupable other)
        {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return NetworkObjectId.GetHashCode();
    }
}