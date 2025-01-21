using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Chest : NetworkBehaviour, IInteractable
{
    private GameObject _chestLid;

    private NetworkVariable<bool> _chestFound = new NetworkVariable<bool>(false);


    private void Awake()
    {
        _chestLid = transform.Find("chest/chest_lid").gameObject;
        if (_chestLid == null)
        {
            Debug.LogError("ChestLid not found!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChestFoundServerRpc()
    {
        ChestFoundClientRpc();
        _chestFound.Value = true;
    }

    [ClientRpc]
    private void ChestFoundClientRpc()
    {
        _chestLid.transform.DORotate(_chestLid.transform.eulerAngles +
                                     new Vector3(-130, 0, 0), 1f)
            .SetEase(Ease.OutBounce);
    }


    public bool Interact()
    {
        if (_chestFound.Value)
        {
            return false;
        }

        ChestFoundServerRpc();
        return true;
    }
}