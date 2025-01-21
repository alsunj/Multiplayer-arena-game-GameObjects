using Unity.Netcode;
using UnityEngine;

public class Explosive : Pickupable, IExplosive
{
    private ExplosiveSettings _explosiveSettings;
    private BoxCollider _boxCollider;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }


    public void Explode()
    {
        throw new System.NotImplementedException();
    }

    public override void Pickup()
    {
        _boxCollider.isTrigger = true;
    }

    public override void PutDown()
    {
        _boxCollider.isTrigger = false;
        //  transform.position = position;
    }
}