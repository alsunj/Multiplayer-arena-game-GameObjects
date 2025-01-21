using System;
using Unity.Netcode;
using UnityEngine;

public abstract class Pickupable : NetworkBehaviour, IEquatable<Pickupable>, IPickupable
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

        _startingRotation = transform.rotation;
    }

    public void RequestPutDownObject(NetworkObjectReference pickupingTarget)
    {
        RequestPutDownObjectServerRpc(pickupingTarget);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPutDownObjectServerRpc(NetworkObjectReference pickupingTarget)
    {
        if (isObjectPickedup.Value)
        {
            isObjectPickedup.Value = false;
            Vector3 newPosition = Vector3.zero;
            if (pickupingTarget.TryGet(out NetworkObject target))
            {
                newPosition = target.transform.position + target.transform.forward;
                PlayerPlacements playerPlacements = target.GetComponent<PlayerPlacements>();
                if (playerPlacements != null)
                {
                    playerPlacements.ClearPlayerRightHandItem();
                }
            }

            // Notify all clients to update their visuals
            PutDownObjectClientRpc(newPosition);
        }
    }

    [ClientRpc]
    private void PutDownObjectClientRpc(Vector3 newPosition)
    {
        _followTransform.SetTargetTransform(null);
        transform.position = newPosition;
        transform.rotation = _startingRotation;
        // Update the visuals on the client
        PutDown();
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
                    playerPlacements.SetPlayerRightHandItem(new NetworkObjectReference(this.NetworkObject));
                    _followTransform.SetTargetTransform(playerPlacements.playerRightHand.transform);
                }
            }

            // Notify all clients to update their visuals
            PickupObjectClientRpc(pickupingTarget);
        }
    }

    [ClientRpc]
    private void PickupObjectClientRpc(NetworkObjectReference pickupingTarget)
    {
        if (pickupingTarget.TryGet(out NetworkObject target))
        {
            PlayerPlacements playerPlacements = target.GetComponent<PlayerPlacements>();
            if (playerPlacements != null)
            {
                _followTransform.SetTargetTransform(playerPlacements.playerRightHand.transform);
            }
        }

        Pickup();
    }

    public abstract void PutDown();

    public abstract void Pickup();

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