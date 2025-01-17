using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Door : NetworkBehaviour, IInteractable
{
    private GameObject _door;

    private NetworkVariable<bool> _doorOpened = new NetworkVariable<bool>(false);

    private void Awake()
    {
        _door = transform.Find("wall_doorway_door").gameObject;
        if (_door == null)
        {
            Debug.LogError("Door not found!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdOpenDoorForEveryoneServerRpc()
    {
        RpcOpenDoorForEveryoneClientRpc();
    }

    [ClientRpc]
    private void RpcOpenDoorForEveryoneClientRpc()
    {
        Interacted();
    }


    [ServerRpc(RequireOwnership = false)]
    private void CmdCloseDoorForEveryoneServerRpc()
    {
        RpcCloseDoorForEveryoneClientRpc();
    }

    [ClientRpc]
    private void RpcCloseDoorForEveryoneClientRpc()
    {
        Interacted();
    }


    private void DoorOpened()
    {
        _door.transform.DORotate(
                new Vector3(0, 90, 0), 1f)
            .SetEase(Ease.OutBounce);
        CmdOpenDoorForEveryoneServerRpc();
    }

    private void DoorOpenedForOtherClients()
    {
        _door.transform.DORotate(
                new Vector3(0, 90, 0), 1f)
            .SetEase(Ease.OutBounce);
    }

    private void DoorClosed()
    {
        _door.transform.DORotate(
                new Vector3(0, 0, 0), 1f)
            .SetEase(Ease.OutBounce);
        CmdCloseDoorForEveryoneServerRpc();
    }

    private void DoorClosedForOtherClients()
    {
        _door.transform.DORotate(
                new Vector3(0, 0, 0), 1f)
            .SetEase(Ease.OutBounce);
    }

    public void Interact()
    {
        RequestOpenDoorServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOpenDoorServerRpc()
    {
        if (!_doorOpened.Value)
        {
            DoorOpened();
            _doorOpened.Value = true;
        }
        else
        {
            DoorClosed();
            _doorOpened.Value = false;
        }
    }

    public void Interacted()
    {
        if (!_doorOpened.Value)
        {
            DoorOpenedForOtherClients();
        }
        else
        {
            DoorClosedForOtherClients();
        }
    }
}