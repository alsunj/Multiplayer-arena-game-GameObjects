using System;
using Unity.Netcode;
using UnityEngine;

public class Key : Pickupable, IPickupable
{
    private Quaternion _startingRotation;
    private FollowTransform _followTransform;
    private BoxCollider _boxCollider;

    private void Awake()
    {
        _followTransform = GetComponent<FollowTransform>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        _startingRotation = gameObject.transform.rotation;
    }

    public override void Pickup(GameObject pickupingTarget)
    {
        GameObject placeHolder = pickupingTarget.GetComponent<PlayerPlacements>().playerRightHand;
        _followTransform.SetTargetTransform(placeHolder.transform);
        _boxCollider.isTrigger = true;
    }


    public override void PutDown(Vector3 position)
    {
        _followTransform.SetTargetTransform(null);
        _boxCollider.isTrigger = false;
        transform.position = position;
        transform.rotation = _startingRotation;
    }
}