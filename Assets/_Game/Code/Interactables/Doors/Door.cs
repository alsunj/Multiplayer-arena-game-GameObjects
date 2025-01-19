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

    [ClientRpc]
    private void DoorOpenClientRpc()
    {
        _door.transform.DORotate(
                new Vector3(0, 90, 0), 1f)
            .SetEase(Ease.OutBounce);
    }


    [ClientRpc]
    private void DoorCloseClientRpc()
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
            DoorOpenClientRpc();
            _doorOpened.Value = true;
        }
        else
        {
            DoorCloseClientRpc();
            _doorOpened.Value = false;
        }
    }
}