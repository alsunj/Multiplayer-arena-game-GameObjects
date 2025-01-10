using System.Numerics;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using UnityEditor;

public class Chest : NetworkBehaviour, IInteractable
{
    private GameObject chestLid;

    [ServerRpc]
    private void CmdOpenChestForEveryoneServerRpc()
    {
        RpcOpenChestForEveryoneClientRpc();
    }

    [ClientRpc]
    private void RpcOpenChestForEveryoneClientRpc()
    {
        Interacted();
    }

    private void Awake()
    {
        chestLid = transform.Find("chest/chest_lid").gameObject;
        if (chestLid == null)
        {
            Debug.LogError("ChestLid not found!");
        }
    }

    private void ChestFound()
    {
        chestLid.transform.DORotate(chestLid.transform.eulerAngles +
                                    new Vector3(-130, 0, 0), 1f)
            .SetEase(Ease.OutBounce);
        CmdOpenChestForEveryoneServerRpc();
    }

    public void Interact()
    {
        ChestFound();
    }

    public void Interacted()
    {
        ChestFound();
    }
}