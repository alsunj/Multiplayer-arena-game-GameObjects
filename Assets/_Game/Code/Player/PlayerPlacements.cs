using Unity.Netcode;
using UnityEngine;

public class PlayerPlacements : NetworkBehaviour
{
    public GameObject playerRightHand;
    public GameObject playerLeftHand;

    private NetworkVariable<NetworkObjectReference> playerRightHandItem = new NetworkVariable<NetworkObjectReference>();
    private NetworkVariable<NetworkObjectReference> playerLeftHandItem = new NetworkVariable<NetworkObjectReference>();

    void Start()
    {
        playerRightHand = transform.Find("PlayerVisual/Skeleton_Minion/Rig/root/handIK.r/EquippedItemR")?.gameObject;
        if (playerRightHand == null)
        {
            Debug.LogError("EquippedItemR not found in player rig");
        }

        playerLeftHand = transform.Find("PlayerVisual/Skeleton_Minion/Rig/root/handIK.l/EquippedItemL")?.gameObject;
        if (playerLeftHand == null)
        {
            Debug.LogError("EquippedItemL not found in player rig");
        }
    }

    [ServerRpc]
    public void SetPlayerRightHandItemServerRpc(NetworkObjectReference newItem)
    {
        if (newItem.TryGet(out NetworkObject networkObject))
        {
            Pickupable pickupable = networkObject.GetComponent<Pickupable>();
            if (pickupable != null)
            {
                playerRightHandItem.Value = newItem;
                UpdatePlayerRightHandItemClientRpc(newItem);
            }
        }
    }

    [ClientRpc]
    private void UpdatePlayerRightHandItemClientRpc(NetworkObjectReference newItem)
    {
        if (newItem.TryGet(out NetworkObject networkObject))
        {
            Pickupable pickupable = networkObject.GetComponent<Pickupable>();
            if (pickupable != null)
            {
                playerRightHandItem.Value = newItem;
            }
        }
    }
}