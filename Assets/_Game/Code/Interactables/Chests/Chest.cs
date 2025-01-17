using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Chest : NetworkBehaviour, IInteractable
{
    private GameObject _chestLid;

    private bool _chestFound;

    [ServerRpc(RequireOwnership = false)]
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
        _chestLid = transform.Find("chest/chest_lid").gameObject;
        if (_chestLid == null)
        {
            Debug.LogError("ChestLid not found!");
        }
    }

    private void ChestFound()
    {
        _chestLid.transform.DORotate(_chestLid.transform.eulerAngles +
                                     new Vector3(-130, 0, 0), 1f)
            .SetEase(Ease.OutBounce);
        CmdOpenChestForEveryoneServerRpc();
    }

    private void ChestFoundForOtherClients()
    {
        _chestLid.transform.DORotate(_chestLid.transform.eulerAngles +
                                     new Vector3(-130, 0, 0), 1f)
            .SetEase(Ease.OutBounce);
    }


    public void Interact()
    {
        if (!_chestFound)
        {
            ChestFound();
            _chestFound = true;
        }
    }

    public void Interacted()
    {
        if (!_chestFound)
        {
            ChestFoundForOtherClients();
            _chestFound = true;
        }
    }
}