using System;
using Unity.Netcode;
using UnityEngine;

public class Key : Pickupable
{
    // private Quaternion _startingRotation;
    private BoxCollider _boxCollider;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }


    public override void Pickup()
    {
        _boxCollider.isTrigger = true;
    }


    public override void PutDown()
    {
        _boxCollider.isTrigger = false;
    }
}