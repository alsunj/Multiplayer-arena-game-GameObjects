using Unity.Netcode;
using UnityEngine;

public abstract class Pickupable : NetworkBehaviour
{
    private NetworkVariable<bool> isObjectPickedup = new NetworkVariable<bool>();


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

    
}